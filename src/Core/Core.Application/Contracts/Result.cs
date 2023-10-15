namespace Core.Application.Contracts;

/// <summary>
/// Represents the result of an operation.
/// If the operation throws an exception internally, <see cref="Failed"/> will be true.
/// </summary>
public struct Result : IResult
{
    public Result()
    {
        Failed = true;
    }

    public Result(bool success)
    {
        Failed = !success;
    }

    public bool Failed { get; private set; }
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// If the operation throws an exception internally, <see cref="Failed"/> will be true.
/// </summary>
public struct Result<T> : IResult
{
    public Result()
    {
        Failed = true;
    }

    public Result(T data)
    {
        Data = data;
    }

    public bool Failed { get; private set; }
    public T? Data { get; private set; }
}
