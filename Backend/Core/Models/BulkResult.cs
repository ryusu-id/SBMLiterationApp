namespace PureTCOWebApp.Core.Models;

public class BulkResult : Result
{
    public BulkResult(bool isSuccess, Error error)
        : base(isSuccess, error)
    {
    }

    public ICollection<Error> Errors { get; private set; } = new List<Error>();

    public new static BulkResult Success() => new BulkResult(true, Error.None);
    public new static BulkResult Failure(Error error) => new BulkResult(false, error);
    public new static BulkResult<T> Success<T>(T value) => new BulkResult<T>(true, Error.None, value);
    public new static BulkResult<T> Failure<T>(Error error) => new BulkResult<T>(false, error, default);

    public BulkResult SetErrors(ICollection<Error> errors)
    {
        Errors = errors;
        return this;
    }
}

public class BulkResult<T> : BulkResult
{
    public T? Value { get; }
    public BulkResult(bool isSuccess, Error error, T? value)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
