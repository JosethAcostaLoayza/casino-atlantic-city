using Control.Application.Interfaces;
using Control.Domain.Entities;

namespace Control.Application.Services;

public class CargaArchivoService
{
    private readonly ICargaArchivoRepository _repository;
    private readonly ICargaMasivaPublisher _publisher;

    public CargaArchivoService(ICargaArchivoRepository repository,ICargaMasivaPublisher publisher){
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<CargaArchivo> RegistrarAsync(string nombreArchivo,string periodo,string usuario){
        if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("El nombre del archivo es obligatorio.");

        if (string.IsNullOrWhiteSpace(periodo)) throw new ArgumentException("El período es obligatorio.");

        if (periodo.Length != 6) throw new ArgumentException("El período debe tener el formato yyyyMM.");

        if (string.IsNullOrWhiteSpace(usuario)) throw new ArgumentException("El usuario es obligatorio.");

        var cargaExistente = await _repository.ObtenerPorPeriodoAsync(periodo);

        if (cargaExistente is not null)
        {
            if (cargaExistente.Estado == "Pendiente" ||
                cargaExistente.Estado == "En proceso")
            {
                throw new InvalidOperationException(
                    $"Ya existe una carga en estado {cargaExistente.Estado} para el período {periodo}.");
            }

            if (cargaExistente.Estado == "Cargado" ||
                cargaExistente.Estado == "Finalizado" ||
                cargaExistente.Estado == "Notificado")
            {
                throw new InvalidOperationException(
                    $"Ya existe una carga registrada para el período {periodo} con estado {cargaExistente.Estado}.");
            }
        }

        var carga = new CargaArchivo(nombreArchivo,periodo,usuario);

        return await _repository.AgregarAsync(carga);
    }

    public async Task ActualizarAsync(CargaArchivo carga)    {
        if (string.IsNullOrWhiteSpace(carga.RutaArchivo))
            throw new InvalidOperationException("La carga no tiene una ruta de archivo.");

        await _repository.ActualizarAsync(carga);
        await _publisher.PublicarAsync(carga.Id,carga.RutaArchivo,carga.Usuario);
    }

    public async Task<CargaArchivo?> ObtenerPorIdAsync(Guid id){
        return await _repository.ObtenerPorIdAsync(id);
    }

    public async Task ActualizarEstadoAsync(Guid id,string estado){
        var carga = await _repository.ObtenerPorIdAsync(id);

        if (carga is null){
            throw new KeyNotFoundException($"No se encontró la carga {id}.");
        }

        carga.CambiarEstado(estado);
        await _repository.ActualizarAsync(carga);
    }

    public async Task<List<CargaArchivo>> ObtenerHistorialAsync()
    {
        return await _repository.ObtenerHistorialAsync();
    }

}