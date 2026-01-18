using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Data;

public static class DataDomainError
{
    public static Error FailedToPersistData(string mesg) => new Error(nameof(FailedToPersistData), mesg);
    public static Error DuplicateEntry => new(nameof(DuplicateEntry), "Failed to save data: Duplicate entry");
}
