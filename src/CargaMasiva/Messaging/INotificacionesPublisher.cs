namespace CargaMasiva.Messaging;

public interface INotificacionesPublisher
{
    Task PublicarAsync(Guid idCarga,string usuario,DateTime fechaFin);
}