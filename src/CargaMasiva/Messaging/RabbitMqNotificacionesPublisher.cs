using System.Text;
using System.Text.Json;
using CargaMasiva.Models;
using RabbitMQ.Client;

namespace CargaMasiva.Messaging;

public class RabbitMqNotificacionesPublisher
    : INotificacionesPublisher
{
    private const string QueueName = "notificaciones";

    private readonly ConnectionFactory _factory;

    public RabbitMqNotificacionesPublisher()
    {
        _factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task PublicarAsync(
        Guid idCarga,
        string usuario,
        DateTime fechaFin)
    {
        await using var connection =
            await _factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var mensaje = new NotificacionMessage
        {
            IdCarga = idCarga,
            Usuario = usuario,
            FechaFin = fechaFin
        };

        var json = JsonSerializer.Serialize(mensaje);

        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: QueueName,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}