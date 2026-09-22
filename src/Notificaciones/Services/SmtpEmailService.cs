using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Notificaciones.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IConfiguration configuration,
        ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo)
    {
        var host = _configuration["Smtp:Host"];
        var port = _configuration.GetValue<int>("Smtp:Port");
        var from = _configuration["Smtp:From"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException(
                "La configuración SMTP está incompleta.");
        }

        var message = new MimeMessage();

        message.From.Add(
            MailboxAddress.Parse(from));

        message.To.Add(
            MailboxAddress.Parse(destinatario));

        message.Subject = asunto;

        message.Body = new TextPart("plain")
        {
            Text = cuerpo
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.None);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);

        _logger.LogInformation(
            "Correo enviado correctamente. Destinatario: {Destinatario}",
            destinatario);
    }
}