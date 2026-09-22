namespace Control.Application.Interfaces;

public interface IFileStorage{
    Task<string> UploadAsync(Stream fileStream,string fileName);
}