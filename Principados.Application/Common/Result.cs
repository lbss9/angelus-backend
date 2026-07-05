namespace Principados.Application.Common;

public class Result<T>
{
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public bool IsSuccess => Error is null;

    private Result(T value) => Value = value;
    private Result(string error) => Error = error;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}
