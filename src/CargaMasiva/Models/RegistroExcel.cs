namespace CargaMasiva.Models;

public class RegistroExcel
{
    public string Periodo { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal? Precio { get; set; }
}