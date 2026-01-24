namespace PureTCOWebApp.Features.FileSystem;

public class MinIOSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
    public string BucketName { get; set; } = "puretco-uploads";
    public string? PublicUrl { get; set; }
}
