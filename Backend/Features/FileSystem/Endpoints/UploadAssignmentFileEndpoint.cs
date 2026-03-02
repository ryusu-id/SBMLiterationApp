using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.FileSystem.Endpoints;

public class UploadAssignmentFileRequest
{
    public IFormFile? File { get; set; }
}

public class UploadAssignmentFileResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

public class UploadAssignmentFileEndpoint : Endpoint<UploadAssignmentFileRequest, ApiResponse<UploadAssignmentFileResponse>>
{
    private readonly IMinIOService _minioService;
    private readonly ILogger<UploadAssignmentFileEndpoint> _logger;

    // Allowed file extensions and their content types
    private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
    {
        { "application/pdf", new[] { ".pdf" } },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new[] { ".docx" } },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new[] { ".xlsx" } },
        { "application/vnd.ms-excel", new[] { ".xls" } },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", new[] { ".pptx" } },
        { "application/vnd.ms-powerpoint", new[] { ".ppt" } },
        { "application/zip", new[] { ".zip" } },
        { "application/x-zip-compressed", new[] { ".zip" } },
    };

    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    public UploadAssignmentFileEndpoint(IMinIOService minioService, ILogger<UploadAssignmentFileEndpoint> logger)
    {
        _minioService = minioService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/assignment/upload");
        Group<FileSystemEndpointGroup>();
        AllowFileUploads();
        Roles("admin");
    }

    public override async Task HandleAsync(UploadAssignmentFileRequest req, CancellationToken ct)
    {
        // Validate file
        if (req.File == null || req.File.Length == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                Result.Failure(new Error("FileNotProvided", "No file was provided for upload"))));
            return;
        }

        // Check file size
        if (req.File.Length > MaxFileSize)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                Result.Failure(new Error("FileTooLarge", $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB"))));
            return;
        }

        // Validate file type
        var extension = Path.GetExtension(req.File.FileName).ToLowerInvariant();
        var contentType = req.File.ContentType.ToLowerInvariant();

        bool isValidType = AllowedFileTypes.Any(kvp =>
            kvp.Key == contentType && kvp.Value.Contains(extension));

        if (!isValidType)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                Result.Failure(new Error("FileTypeNotAllowed", $"File type not allowed. Allowed types: {string.Join(", ", AllowedFileTypes.SelectMany(x => x.Value))}"))));
            return;
        }

        try
        {
            // Upload file to MinIO
            using var stream = req.File.OpenReadStream();
            var url = await _minioService.UploadFileAsync(stream, req.File.FileName, contentType, ct);

            var response = new UploadAssignmentFileResponse
            {
                Url = url,
                FileName = req.File.FileName,
                FileSize = req.File.Length,
                ContentType = contentType
            };

            _logger.LogInformation("File uploaded successfully: {FileName}, Size: {FileSize}, URL: {Url}",
                req.File.FileName, req.File.Length, url);

            await Send.OkAsync(Result.Success(response), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", req.File.FileName);
            await Send.ResultAsync(TypedResults.InternalServerError<ApiResponse>(
                Result.Failure(new Error("InternalError", "An error occurred while uploading the file"))));
        }
    }
}
