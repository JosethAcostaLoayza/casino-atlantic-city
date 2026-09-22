using Control.Application.Interfaces;
using Control.Application.Services;
using Control.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Control.Application.DTOs;

namespace Control.Controllers;

[ApiController]
[Route("api/cargas")]
[Authorize]
public class CargasController : ControllerBase
{
    private readonly CargaArchivoService _service;
    private readonly IFileStorage _fileStorage;
    private readonly IConfiguration _configuration;

    public CargasController(CargaArchivoService service,IFileStorage fileStorage,IConfiguration configuration){
        _service = service;
        _fileStorage = fileStorage;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] CrearCargaRequest request,IFormFile archivo){
        if (archivo is null || archivo.Length == 0){
            return BadRequest(new{message = "Debe enviar un archivo."});
        }

        var extension = Path.GetExtension(archivo.FileName);

        if (!string.Equals(extension,".xlsx",StringComparison.OrdinalIgnoreCase)){
            return BadRequest(new{message = "El archivo debe tener formato .xlsx."
            });
        }

        var maxFileSizeMb =_configuration.GetValue<long>("FileUpload:MaxFileSizeMb");
        var maxFileSizeBytes =maxFileSizeMb * 1024 * 1024;

        if (archivo.Length > maxFileSizeBytes){
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new{
                    message =$"El archivo supera el tamaño máximo permitido de {maxFileSizeMb} MB."
                });
        }

        var usuario = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(usuario)){
            return Unauthorized(new{message = "No se pudo identificar al usuario."
            });
        }

            var carga = await _service.RegistrarAsync(archivo.FileName,request.Periodo,usuario);
            await using var stream = archivo.OpenReadStream();
            var rutaArchivo = await _fileStorage.UploadAsync(stream,archivo.FileName);
            carga.AsignarRutaArchivo(rutaArchivo);
            await _service.ActualizarAsync(carga);

            return CreatedAtAction(
                nameof(Obtener),
                new { id = carga.Id },
                new
                {
                    carga.Id,
                    carga.NombreArchivo,
                    carga.Periodo,
                    carga.Estado,
                    carga.FechaRegistro,
                    carga.Usuario,
                    carga.RutaArchivo
                });

    }

    [HttpGet]
    public async Task<IActionResult> ObtenerHistorial()
    {
        var cargas = await _service.ObtenerHistorialAsync();

        return Ok(cargas);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id){
        var carga = await _service.ObtenerPorIdAsync(id);

        if (carga is null){
            return NotFound(new{
                message = "Carga no encontrada."
            });
        }

        return Ok(new
        {
            carga.Id,
            carga.NombreArchivo,
            carga.Periodo,
            carga.Estado,
            carga.FechaRegistro,
            carga.FechaFin,
            carga.Usuario,
            carga.RutaArchivo,
        });
    }

    [HttpPatch("{id:guid}/estado")]
    [AllowAnonymous]
    public async Task<IActionResult> ActualizarEstado(Guid id,[FromBody] ActualizarEstadoRequest request,[FromHeader(Name = "X-Internal-Key")] string? internalKey){
        var configuredKey = _configuration["InternalApiKey"];

        if (string.IsNullOrWhiteSpace(internalKey) || internalKey != configuredKey){
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Estado)){
            return BadRequest("El estado es obligatorio.");
        }

        await _service.ActualizarEstadoAsync(id,request.Estado);

        return NoContent();
    }
}