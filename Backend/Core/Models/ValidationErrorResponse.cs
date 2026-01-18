namespace PureTCOWebApp.Core.Models;

public record ValidationErrorResponse(
    string? Message,
    string? ErrorCode,
    string? ErrorDescription,
    IDictionary<string, string[]> Errors
);
