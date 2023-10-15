namespace Core.Application.Contracts;

/// <summary>
/// Represents the result of an operation.
/// If the operation throws an exception internally, <see cref="Failed"/> will be true.
/// </summary>
public interface IResult
{
    bool Failed { get; }
}
