namespace PureTCOWebApp.Features.FileSystem;

public interface IMinIOService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string fileName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
    Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default);
}
