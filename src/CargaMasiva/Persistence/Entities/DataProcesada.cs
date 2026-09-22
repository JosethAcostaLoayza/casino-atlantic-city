namespace CargaMasiva.Persistence.Entities;

public class DataProcesada
{
    public Guid Id { get; set; }

    public Guid IdCarga { get; set; }

    public string Periodo { get; set; } = string.Empty;

    public string CodigoProducto { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public DateTime FechaRegistro { get; set; }
}