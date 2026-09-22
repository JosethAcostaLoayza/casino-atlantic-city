namespace CargaMasiva.Models;

public class CargaMasivaMessage
{
    public Guid IdCarga { get; set; }

    public string RutaArchivo { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;
}