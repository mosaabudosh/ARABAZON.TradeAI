namespace ARABAZON.TradeAI.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string error, string errorCode) { IsSuccess = false; Error = error; ErrorCode = errorCode; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, string errorCode = "ERROR") => new(error, errorCode);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result() { IsSuccess = true; }
    private Result(string error, string errorCode) { IsSuccess = false; Error = error; ErrorCode = errorCode; }

    public static Result Success() => new();
    public static Result Failure(string error, string errorCode = "ERROR") => new(error, errorCode);
}