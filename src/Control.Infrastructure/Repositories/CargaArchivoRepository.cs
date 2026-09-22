using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public class CargaArchivoRepository : ICargaArchivoRepository
{
    private readonly ControlDbContext _context;

    public CargaArchivoRepository(ControlDbContext context){
        _context = context;
    }

    public async Task<CargaArchivo?> ObtenerPorPeriodoAsync(string periodo){
        return await _context.Cargas
            .Where(c => c.Periodo == periodo)
            .OrderByDescending(c => c.FechaRegistro)
            .FirstOrDefaultAsync();
    }

    public async Task<CargaArchivo> AgregarAsync(CargaArchivo carga){
        await _context.Cargas.AddAsync(carga);
        await _context.SaveChangesAsync();
        return carga;
    }

    public async Task<CargaArchivo?> ObtenerPorIdAsync(Guid id){
    return await _context.Cargas.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task ActualizarAsync(CargaArchivo carga){
    _context.Cargas.Update(carga);
    await _context.SaveChangesAsync();
    }

    public async Task ActualizarEstadoAsync(Guid id, string estado){
        var carga = await _context.Cargas.FirstOrDefaultAsync(c => c.Id == id);

        if (carga == null){
            throw new KeyNotFoundException($"No se encontró la carga con Id {id}");
        }

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            CALL sp_actualizar_estado_carga(
                {id},
                {estado}
            )
            """);
    }

    public async Task<List<CargaArchivo>> ObtenerHistorialAsync()
    {
        return await _context.Cargas
            .OrderByDescending(c => c.FechaRegistro)
            .ToListAsync();
    }

}