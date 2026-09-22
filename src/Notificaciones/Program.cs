using Notificaciones.Integration;
using Notificaciones.Messaging;
using Notificaciones.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<ControlStatusClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Control:BaseUrl"]!);
});

builder.Services.AddSingleton<
    IEmailService,
    SmtpEmailService>();

builder.Services.AddHostedService<
    RabbitMqNotificacionesConsumer>();

var host = builder.Build();

host.Run();