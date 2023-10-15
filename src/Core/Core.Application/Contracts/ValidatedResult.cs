using FluentValidation.Results;

namespace Core.Application.Contracts;

/// <summary>
/// Represents the status of an operation that includes validation.
/// If the validation fails, <see cref="Status"/> will be set to <see cref="ResultStatus.Invalid"/>.
/// If an exception occurs, <see cref="Status"/> will be set to <see cref="ResultStatus.Error"/>.
/// </summary>
public struct ValidatedResult : IValidatedResult
{
    public ValidatedResult(ResultStatus status)
    {
        if (status == ResultStatus.Invalid)
        {
            throw new ArgumentException($"Use this constructor only with {nameof(ResultStatus.Successful)} and {nameof(ResultStatus.Error)} status");
        }

        Status = status;
    }

    public ValidatedResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyCollection<ValidationFailure>? ValidationErrors { get; private set; }
}

/// <summary>
/// Represents the status of an operation that includes validation and returns a value.
/// If the validation fails, <see cref="Status"/> will be set to <see cref="ResultStatus.Invalid"/>.
/// If an exception occurs, <see cref="Status"/> will be set to <see cref="ResultStatus.Error"/>.
/// </summary>
public struct ValidatedResult<T> : IValidatedResult
{
    public ValidatedResult()
    {
        Status = ResultStatus.Error;
    }

    public ValidatedResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ValidatedResult(T data)
    {
        Data = data;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyCollection<ValidationFailure>? ValidationErrors { get; private set; }
    public T? Data { get; private set; }
}
