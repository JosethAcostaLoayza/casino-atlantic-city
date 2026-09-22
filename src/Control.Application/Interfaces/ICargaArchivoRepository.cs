using Control.Domain.Entities;

namespace Control.Application.Interfaces;

public interface ICargaArchivoRepository
{
    Task<CargaArchivo?> ObtenerPorPeriodoAsync(string periodo);
    Task<CargaArchivo> AgregarAsync(CargaArchivo carga);
    Task<CargaArchivo?> ObtenerPorIdAsync(Guid id);
    Task ActualizarAsync(CargaArchivo carga);
    Task ActualizarEstadoAsync(Guid id, string estado);
    Task<List<CargaArchivo>> ObtenerHistorialAsync();
}