using System.Net.Http.Json;

namespace Notificaciones.Integration;

public class ControlStatusClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ControlStatusClient> _logger;

    public ControlStatusClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ControlStatusClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ActualizarEstadoAsync(
        Guid idCarga,
        string estado,
        CancellationToken cancellationToken = default)
    {
        var internalKey =
            _configuration["Control:InternalApiKey"];

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/cargas/{idCarga}/estado");

        request.Headers.Add(
            "X-Internal-Key",
            internalKey);

        request.Content = JsonContent.Create(new
        {
            estado
        });

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "Estado de carga actualizado. IdCarga: {IdCarga}, Estado: {Estado}",
            idCarga,
            estado);
    }
}