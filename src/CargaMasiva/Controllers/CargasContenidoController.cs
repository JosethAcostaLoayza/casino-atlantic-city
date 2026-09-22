using CargaMasiva.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CargaMasiva.Controllers;

[ApiController]
[Route("api/cargas")]
public class CargasContenidoController : ControllerBase
{
    private readonly CargaMasivaDbContext _db;

    public CargasContenidoController(CargaMasivaDbContext db)
    {
        _db = db;
    }

    [HttpGet("{idCarga:guid}/contenido")]
    public async Task<IActionResult> ObtenerContenido(
        Guid idCarga,
        CancellationToken cancellationToken)
    {
        var registros = await _db.DataProcesada
            .AsNoTracking()
            .Where(x => x.IdCarga == idCarga)
            .OrderBy(x => x.FechaRegistro)
            .Select(x => new
            {
                x.Id,
                x.IdCarga,
                x.Periodo,
                x.CodigoProducto,
                x.Descripcion,
                x.Precio,
                x.FechaRegistro
            })
            .ToListAsync(cancellationToken);

        var errores = await _db.DetalleCarga
            .AsNoTracking()
            .Where(x => x.IdCarga == idCarga)
            .OrderBy(x => x.Fila)
            .Select(x => new
            {
                x.Id,
                x.IdCarga,
                x.Fila,
                x.CodigoProducto,
                x.Observacion,
                x.FechaRegistro
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            idCarga,
            registros,
            errores
        });
    }
}