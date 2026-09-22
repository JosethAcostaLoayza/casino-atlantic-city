namespace Control.Domain.Entities;

public class CargaArchivo
{
    public Guid Id { get; private set; }
    public string NombreArchivo { get; private set; }
    public string Periodo { get; private set; }
    public string Estado { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public string Usuario { get; private set; }
    public string? RutaArchivo { get; private set; }
    public DateTime? FechaFin { get; private set; }

    public CargaArchivo(string nombreArchivo,string periodo,string usuario){
        Id = Guid.NewGuid();
        NombreArchivo = nombreArchivo;
        Periodo = periodo;
        Estado = "Pendiente";
        FechaRegistro = DateTime.UtcNow;
        Usuario = usuario;
    }

    public void CambiarEstado(string estado){
        Estado = estado;
        
        if (estado == "Finalizado"){
        FechaFin = DateTime.UtcNow;
        }
    }

    public void AsignarRutaArchivo(string rutaArchivo){
        RutaArchivo = rutaArchivo;
    }
}