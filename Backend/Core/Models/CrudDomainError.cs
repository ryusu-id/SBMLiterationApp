namespace PureTCOWebApp.Core.Models;

public static class CrudDomainError
{
    public static Error CreateFailed(string entityName, string? reason = null)
        => new(nameof(CreateFailed), $"Failed to create {entityName}{FormatReason(reason)}");

    public static Error ReadFailed(string entityName, string? reason = null)
        => new(nameof(ReadFailed), $"Failed to read {entityName}{FormatReason(reason)}");

    public static Error UpdateFailed(string entityName, string? reason = null)
        => new(nameof(UpdateFailed), $"Failed to update {entityName}{FormatReason(reason)}");

    public static Error DeleteFailed(string entityName, string? reason = null)
        => new(nameof(DeleteFailed), $"Failed to delete {entityName}{FormatReason(reason)}");

    public static Error NotFound(string entityName, object? key = null)
        => new(nameof(NotFound), key == null
            ? $"{entityName} not found."
            : $"{entityName} not found.");

    public static Error Duplicate(string entityName, string? fieldName = null)
        => new(nameof(Duplicate), fieldName == null
            ? $"{entityName} already exists."
            : $"{entityName} with the same {fieldName} already exists.");

    public static Error MisMatchParamAndBody(string paramName, string bodyName)
        => new(nameof(MisMatchParamAndBody), $"Route parameter '{paramName}' does not match body property '{bodyName}'.");

    private static string FormatReason(string? reason)
        => string.IsNullOrWhiteSpace(reason) ? "." : $": {reason}";

    public static Error OneOrMoreValidationError => new(nameof(OneOrMoreValidationError), "One or more validation errors occurred.");

    public static Error ValidationError(string fieldName, string message)
        => new(nameof(ValidationError), $"{fieldName}: {message}");

    public static Error FieldValidation(string fieldName, string message)
    => new Error(nameof(ValidationError), "One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { message } }
        }
    };
}
