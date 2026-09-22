using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence;

public class ControlDbContext : DbContext
{
    public ControlDbContext(DbContextOptions<ControlDbContext> options)
        : base(options)
    {
    }

    public DbSet<CargaArchivo> Cargas => Set<CargaArchivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<CargaArchivo>(entity =>{
            entity.ToTable("CargasArchivo");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.NombreArchivo).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Periodo).IsRequired().HasMaxLength(6);
            entity.Property(c => c.Estado).IsRequired().HasMaxLength(30);
            entity.Property(c => c.Usuario).IsRequired().HasMaxLength(150);
            entity.Property(c => c.RutaArchivo).HasMaxLength(500);
            entity.Property(c => c.FechaRegistro).IsRequired();
            entity.HasIndex(c => c.Periodo);
            entity.Property(x => x.FechaFin);
        });
    }
}