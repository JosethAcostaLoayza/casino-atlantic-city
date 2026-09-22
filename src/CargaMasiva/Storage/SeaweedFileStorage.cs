namespace CargaMasiva.Storage;

public class SeaweedFileStorage
{
    private readonly HttpClient _httpClient;

    public SeaweedFileStorage(HttpClient httpClient){
        _httpClient = httpClient;
    }

    public async Task<byte[]> DownloadAsync(string rutaArchivo){
        var response = await _httpClient.GetAsync(rutaArchivo);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}