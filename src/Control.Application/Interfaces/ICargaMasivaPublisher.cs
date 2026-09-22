namespace Control.Application.Interfaces;

public interface ICargaMasivaPublisher
{
    Task PublicarAsync(Guid idCarga,string rutaArchivo,string usuario);
}