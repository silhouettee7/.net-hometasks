namespace RESTAuth.Domain.Abstractions.Services;

public interface IFileStorageService
{
    Task UploadFileAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
    Task<string> GetFileLinkAsync(string fileName, CancellationToken cancellationToken = default);
}