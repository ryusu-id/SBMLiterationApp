using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Core;

public record ApiResponse(
    string? Message,
    string? ErrorCode,
    string? ErrorDescription,
    string[]? Errors = null
)
{
    public static implicit operator ApiResponse(Result result) => result.IsSuccess
        ? new ApiResponse(result.Message, null, null)
        : new ApiResponse(null, result.Error.Code, result.Error.Description);

    public static implicit operator ApiResponse(BulkResult result) => result.IsSuccess
        ? new ApiResponse(result.Message, null, null)
        : new ApiResponse(null, result.Error.Code, result.Error.Description, result.Errors.Select(e => e.Description!).ToArray());
}

public record ApiResponse<TData>(
    string? Message,
    TData? Data,
    string? ErrorCode,
    string? ErrorDescription,
    string[]? Errors = null
)
{
    public static implicit operator ApiResponse<TData>(Result<TData> result) => result.IsSuccess
        ? new ApiResponse<TData>(result.Message, result.Value, null, null)
        : new ApiResponse<TData>(null, default, result.Error.Code, result.Error.Description);

    public static implicit operator ApiResponse<TData>(BulkResult<TData> result) => result.IsSuccess
        ? new ApiResponse<TData>(result.Message, result.Value, null, null)
        : new ApiResponse<TData>(null, default, result.Error.Code, result.Error.Description, [.. result.Errors.Select(e => e.Description!)]);
}
