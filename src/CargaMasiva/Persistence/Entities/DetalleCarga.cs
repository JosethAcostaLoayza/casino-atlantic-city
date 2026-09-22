namespace CargaMasiva.Persistence.Entities;

public class DetalleCarga
{
    public Guid Id { get; set; }

    public Guid IdCarga { get; set; }

    public int Fila { get; set; }

    public string CodigoProducto { get; set; } = string.Empty;

    public string Observacion { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }
}