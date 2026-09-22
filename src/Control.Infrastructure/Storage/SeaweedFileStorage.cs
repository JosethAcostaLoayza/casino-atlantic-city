using Control.Application.Interfaces;

namespace Control.Infrastructure.Storage;

public class SeaweedFileStorage : IFileStorage{
    private readonly HttpClient _httpClient;

    public SeaweedFileStorage(HttpClient httpClient){
        _httpClient = httpClient;
    }

    public async Task<string> UploadAsync(Stream fileStream,string fileName){

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);

        content.Add(fileContent, "file", fileName);

        using var response = await _httpClient.PostAsync("/cargas/",content);
        response.EnsureSuccessStatusCode();

        return $"/cargas/{fileName}";
    }
}