namespace CargaMasiva.Models;

public class ResultadoRegistro
{
    public int Fila { get; set; }
    public RegistroExcel Registro { get; set; } = new();

    public bool EsValido { get; set; }

    public string? Observacion { get; set; }
}