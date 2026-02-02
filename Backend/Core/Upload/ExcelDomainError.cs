
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Core.Upload;

public static class ExcelDomainError
{
    public static Error FileIsEmpty => new(nameof(FileIsEmpty), "File is empty, Pleae upload a valid Excel file.");
    public static Error FileSizeExceedsLimit(int sizeInMB) => new(nameof(FileSizeExceedsLimit), $"File size exceeds the limit of {sizeInMB} MB.");
    public static Error InvalidFileType => new(nameof(InvalidFileType), "Invalid file type. Please upload a valid Excel file (.xlsx).");
    public static Error ValidationError(string cell, string message) 
        => new(nameof(ValidationError), $"{cell}: {message}");
    public static Error OneOrMoreValidationError => new(nameof(OneOrMoreValidationError), "One or more validation errors occurred.");
}
