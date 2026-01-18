namespace PureTCOWebApp.Core.Models;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; protected set; }
    
    public string Message { get; } = string.Empty;
    public static Result Success(string message) => new(true, message, Error.None);
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new Result<T>(true, Error.None, value);
    public static Result<T> Failure<T>(Error error) => new Result<T>(false, error, default);
    protected Result(bool isSuccess, string message, Error error)
    {
        Message = message;
        IsSuccess = isSuccess;
        Error = error;
    }
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public static Result FromResult(Result result)
    {
        return result;
    }
}

public class Result<T> : Result
{
    public T? Value { get; }
    public Result(bool isSuccess, Error error, T? value)
        : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static new Result FromResult(Result result)
    {
        return new Result<T>(result.IsSuccess, result.Error, default);
    }
}
