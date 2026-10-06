namespace SeoLeadPlatform.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? code = null) => new(false, error, code);
    public static Result<T> Success<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Failure<T>(string error, string? code = null) => Result<T>.Fail(error, code);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, string? error, string? code)
        : base(isSuccess, error, code)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new(value, true, null, null);
    public static Result<T> Fail(string error, string? code = null) => new(default, false, error, code);
}
