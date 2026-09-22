using System.Text;
using System.Text.Json;
using CargaMasiva.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CargaMasiva.Storage;
using CargaMasiva.Processing;
using CargaMasiva.Persistence;
using CargaMasiva.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using CargaMasiva.Integration;

namespace CargaMasiva.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private const string QueueName = "carga_masiva";

    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly SeaweedFileStorage _fileStorage;
    private readonly ExcelProcessor _excelProcessor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ControlStatusClient _controlStatusClient;
    private readonly INotificacionesPublisher _notificacionesPublisher;

    public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger,SeaweedFileStorage fileStorage,ExcelProcessor excelProcessor,IServiceScopeFactory scopeFactory,ControlStatusClient controlStatusClient,INotificacionesPublisher notificacionesPublisher)
    {
        _logger = logger;
        _fileStorage = fileStorage;
        _excelProcessor = excelProcessor;
        _scopeFactory = scopeFactory;
        _controlStatusClient = controlStatusClient;
        _notificacionesPublisher = notificacionesPublisher;
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ClientProvidedName = "CargaMasiva"
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

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);

                        _logger.LogInformation(
                            "Mensaje recibido: {Json}",
                            json);

                        var opciones = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var mensaje =
                            JsonSerializer.Deserialize<CargaMasivaMessage>(
                                json,
                                opciones);

                        if (mensaje is null)
                        {
                            throw new InvalidOperationException(
                                "No se pudo interpretar el mensaje.");
                        }

                        // Actualizar a: En proceso
                        await _controlStatusClient.ActualizarEstadoAsync(
                            mensaje.IdCarga,
                            "En proceso",
                            stoppingToken);

                        _logger.LogInformation(
                            "Carga recibida. IdCarga: {IdCarga}, Usuario: {Usuario}, Ruta: {RutaArchivo}",
                            mensaje.IdCarga,
                            mensaje.Usuario,
                            mensaje.RutaArchivo);

                        var archivo =
                            await _fileStorage.DownloadAsync(
                                mensaje.RutaArchivo);

                        _logger.LogInformation(
                            "Archivo descargado correctamente. Tamaño: {Tamanio} bytes",
                            archivo.Length);

                        var registros =
                            _excelProcessor.Procesar(archivo);

                        _logger.LogInformation(
                            "Archivo procesado. Registros obtenidos: {Cantidad}",
                            registros.Count);

                        using var scope =
                            _scopeFactory.CreateScope();

                        var dbContext =
                            scope.ServiceProvider
                                .GetRequiredService<CargaMasivaDbContext>();

                        var codigosExcel = registros
                            .Where(x =>
                                x.EsValido &&
                                !string.IsNullOrWhiteSpace(
                                    x.Registro.CodigoProducto))
                            .Select(x =>
                                x.Registro.CodigoProducto)
                            .Distinct()
                            .ToList();

                        var codigosExistentes =
                            await dbContext.DataProcesada
                                .Where(x =>
                                    codigosExcel.Contains(
                                        x.CodigoProducto))
                                .Select(x =>
                                    x.CodigoProducto)
                                .ToListAsync();

                        var codigosExistentesSet =
                            new HashSet<string>(
                                codigosExistentes);

                        foreach (var resultado in registros)
                        {
                            if (resultado.EsValido)
                            {
                                var codigoProducto =
                                    resultado.Registro.CodigoProducto;

                                if (codigosExistentesSet.Contains(
                                    codigoProducto))
                                {
                                    var detalleExistente =
                                        new DetalleCarga
                                        {
                                            Id = Guid.NewGuid(),
                                            IdCarga = mensaje.IdCarga,
                                            Fila = resultado.Fila,
                                            CodigoProducto =
                                                codigoProducto,
                                            Observacion = "Existente",
                                            FechaRegistro =
                                                DateTime.UtcNow
                                        };

                                    dbContext.DetalleCarga.Add(
                                        detalleExistente);

                                    _logger.LogWarning(
                                        "CodigoProducto existente. No se registrará nuevamente: {CodigoProducto}",
                                        codigoProducto);

                                    continue;
                                }

                                var data = new DataProcesada
                                {
                                    Id = Guid.NewGuid(),
                                    IdCarga = mensaje.IdCarga,
                                    Periodo =
                                        resultado.Registro.Periodo,
                                    CodigoProducto =
                                        codigoProducto,
                                    Descripcion =
                                        resultado.Registro.Descripcion,
                                    Precio =
                                        resultado.Registro.Precio ?? 0,
                                    FechaRegistro =
                                        DateTime.UtcNow
                                };

                                dbContext.DataProcesada.Add(data);
                            }
                            else
                            {
                                var detalle = new DetalleCarga
                                {
                                    Id = Guid.NewGuid(),
                                    IdCarga = mensaje.IdCarga,
                                    Fila = resultado.Fila,
                                    CodigoProducto =
                                        resultado.Registro.CodigoProducto,
                                    Observacion =
                                        resultado.Observacion
                                        ?? "Registro inválido",
                                    FechaRegistro =
                                        DateTime.UtcNow
                                };

                                dbContext.DetalleCarga.Add(
                                    detalle);
                            }
                        }

                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation(
                            "Datos guardados correctamente en PostgreSQL.");

                        // Actualizar a: Cargado
                        await _controlStatusClient.ActualizarEstadoAsync(
                            mensaje.IdCarga,
                            "Cargado",
                            stoppingToken);

                        // Actualizar a: Finalizado
                        await _controlStatusClient.ActualizarEstadoAsync(
                            mensaje.IdCarga,
                            "Finalizado",
                            stoppingToken);

                        var fechaFin = DateTime.UtcNow;

                        await _notificacionesPublisher.PublicarAsync(
                            mensaje.IdCarga,
                            mensaje.Usuario,
                            fechaFin);

                        _logger.LogInformation(
                            "Notificación publicada. IdCarga: {IdCarga}, Usuario: {Usuario}",
                            mensaje.IdCarga,
                            mensaje.Usuario);

                        await channel.BasicAckAsync(
                            ea.DeliveryTag,
                            multiple: false);

                        _logger.LogInformation(
                            "Mensaje confirmado con ACK. IdCarga: {IdCarga}",
                            mensaje.IdCarga);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error procesando mensaje de carga masiva.");

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