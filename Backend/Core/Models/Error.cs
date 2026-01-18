namespace PureTCOWebApp.Core.Models;

public sealed record Error(
    string Code, 
    string? Description = null, 
    Type? ErrorType = null,
    params object[] Args)
{
    public Dictionary<string, string[]>? Errors { get; init; }
    public static readonly Error None = new(string.Empty);
    public static implicit operator Result(Error error) => Result.Failure(error);

    public static Error CreateError<T>(string code, string? description = null, params object[] args)
    {
        return new Error(code, description, typeof(T), args);
    }

    public bool Equals(Error? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        return Code == other.Code;
    }

    public override int GetHashCode() => Code.GetHashCode();
}
