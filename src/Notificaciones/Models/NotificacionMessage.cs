namespace Notificaciones.Models;

public class NotificacionMessage
{
    public Guid IdCarga { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public DateTime FechaFin { get; set; }
}