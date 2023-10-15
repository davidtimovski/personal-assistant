using FluentValidation.Results;

namespace Core.Application.Contracts;

/// <summary>
/// Represents the status of an operation that includes validation.
/// If the validation fails, <see cref="Invalid"/> should be used.
/// If an exception occurs, <see cref="Error"/> should be used.
/// </summary>
public enum ResultStatus
{
    Successful,
    Invalid,
    Error
}
