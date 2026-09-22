using System.Text;
using System.Text.Json;
using Control.Application.Interfaces;
using RabbitMQ.Client;

namespace Control.Infrastructure.Messaging;

public class RabbitMqCargaMasivaPublisher : ICargaMasivaPublisher
{
    private const string QueueName = "carga_masiva";

    private readonly ConnectionFactory _factory;

    public RabbitMqCargaMasivaPublisher()
    {
        _factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task PublicarAsync(Guid idCarga,string rutaArchivo,string usuario){
        await using var connection =await _factory.CreateConnectionAsync();
        await using var channel =await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: QueueName,durable: true,exclusive: false,autoDelete: false);

        var mensaje = new{idCarga,rutaArchivo,usuario};
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