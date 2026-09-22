using System.Text;
using System.Text.Json;
using Notificaciones.Integration;
using Notificaciones.Models;
using Notificaciones.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notificaciones.Messaging;

public class RabbitMqNotificacionesConsumer : BackgroundService
{
    private const string QueueName = "notificaciones";

    private readonly ILogger<RabbitMqNotificacionesConsumer> _logger;
    private readonly IEmailService _emailService;
    private readonly ControlStatusClient _controlStatusClient;

    public RabbitMqNotificacionesConsumer(
        ILogger<RabbitMqNotificacionesConsumer> logger,
        IEmailService emailService,
        ControlStatusClient controlStatusClient)
    {
        _logger = logger;
        _emailService = emailService;
        _controlStatusClient = controlStatusClient;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ClientProvidedName = "Notificaciones"
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection =
                    await factory.CreateConnectionAsync(stoppingToken);

                await using var channel =
                    await connection.CreateChannelAsync(
                        cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                var consumer =
                    new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();

                        var json = Encoding.UTF8.GetString(body);

                        _logger.LogInformation(
                            "Mensaje de notificación recibido: {Json}",
                            json);

                        var opciones = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var mensaje =
                            JsonSerializer.Deserialize<NotificacionMessage>(
                                json,
                                opciones);

                        if (mensaje is null)
                        {
                            throw new InvalidOperationException(
                                "No se pudo interpretar el mensaje de notificación.");
                        }

                        _logger.LogInformation(
                            "Procesando notificación. IdCarga: {IdCarga}, Usuario: {Usuario}",
                            mensaje.IdCarga,
                            mensaje.Usuario);

                        var asunto =
                            $"Carga masiva finalizada - {mensaje.IdCarga}";

                        var cuerpo =
                            $"""
                            La carga masiva ha finalizado correctamente.

                            Id de carga: {mensaje.IdCarga}
                            Fecha de finalización: {mensaje.FechaFin:yyyy-MM-dd HH:mm:ss} UTC
                            """;

                        await _emailService.EnviarAsync(
                            mensaje.Usuario,
                            asunto,
                            cuerpo);

                        await _controlStatusClient.ActualizarEstadoAsync(
                            mensaje.IdCarga,
                            "Notificado",
                            stoppingToken);

                        await channel.BasicAckAsync(
                            ea.DeliveryTag,
                            multiple: false);

                        _logger.LogInformation(
                            "Notificación procesada y confirmada con ACK. IdCarga: {IdCarga}",
                            mensaje.IdCarga);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error procesando notificación.");

                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            multiple: false,
                            requeue: false);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Consumidor de RabbitMQ iniciado. Cola: {QueueName}",
                    QueueName);

                await Task.Delay(
                    Timeout.Infinite,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (
                stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo conectar a RabbitMQ. Reintentando en 5 segundos...");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }

}