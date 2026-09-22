using CargaMasiva.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargaMasiva.Persistence;

public class CargaMasivaDbContext : DbContext
{
    public CargaMasivaDbContext(
        DbContextOptions<CargaMasivaDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataProcesada> DataProcesada => Set<DataProcesada>();

    public DbSet<DetalleCarga> DetalleCarga => Set<DetalleCarga>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProcesada>(entity =>
        {
            entity.ToTable("DataProcesada");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Periodo)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(x => x.CodigoProducto)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Descripcion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.Precio)
                .HasColumnType("numeric(18,2)");

            entity.Property(x => x.FechaRegistro)
                .IsRequired();
        });

        modelBuilder.Entity<DetalleCarga>(entity =>
        {
            entity.ToTable("DetalleCarga");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CodigoProducto)
                .HasMaxLength(100);

            entity.Property(x => x.Observacion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.FechaRegistro)
                .IsRequired();
        });
    }
}