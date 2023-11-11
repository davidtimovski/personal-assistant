using FluentValidation.Results;

namespace Core.Application.Contracts;

/// <summary>
/// Represents the status of an operation that includes validation.
/// If the validation fails, <see cref="Status"/> will be set to <see cref="ResultStatus.Invalid"/>.
/// If an exception occurs, <see cref="Status"/> will be set to <see cref="ResultStatus.Error"/>.
/// </summary>
public interface IValidatedResult
{
    ResultStatus Status { get; }
    IReadOnlyList<ValidationFailure>? ValidationErrors { get; }
}
