using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace PureTCOWebApp.Features.FileSystem;

public class MinIOService : IMinIOService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIOSettings _settings;
    private readonly ILogger<MinIOService> _logger;

    public MinIOService(IMinioClient minioClient, IOptions<MinIOSettings> settings, ILogger<MinIOService> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(_settings.BucketName);

            bool exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_settings.BucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
                
                // Set bucket policy to public read
                string policy = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"": [""*""]}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{_settings.BucketName}/*""]
                        }}
                    ]
                }}";

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(_settings.BucketName)
                    .WithPolicy(policy);

                await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
                
                _logger.LogInformation("Bucket {BucketName} created and set to public read", _settings.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", _settings.BucketName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            // Return public URL
            var publicUrl = GetPublicUrl(uniqueFileName);
            
            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            
            _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(fileName)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for file: {FileName}", fileName);
            throw;
        }
    }

    private string GetPublicUrl(string fileName)
    {
        // Use public URL if configured, otherwise construct from endpoint
        if (!string.IsNullOrEmpty(_settings.PublicUrl))
        {
            return $"{_settings.PublicUrl.TrimEnd('/')}/{_settings.BucketName}/{fileName}";
        }

        var protocol = _settings.UseSSL ? "https" : "http";
        return $"{protocol}://{_settings.Endpoint}/{_settings.BucketName}/{fileName}";
    }
}
