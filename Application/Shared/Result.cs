namespace Application.Shared;

public class Result<T, TError>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public TError? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(TError error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T, TError> Success(T value) => new(value);
    public static Result<T, TError> Failure(TError error) => new(error);
}
