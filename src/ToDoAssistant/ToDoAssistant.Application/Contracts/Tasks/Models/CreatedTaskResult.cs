using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation.Results;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class CreatedTaskResult : INotificationResult, IValidatedResult
{
    public CreatedTaskResult(ResultStatus status)
    {
        if (status == ResultStatus.Invalid)
        {
            throw new ArgumentException($"Use this constructor only with {nameof(ResultStatus.Successful)} and {nameof(ResultStatus.Error)} status");
        }

        Status = status;
    }

    public CreatedTaskResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyList<ValidationFailure>? ValidationErrors { get; private set; }

    public int TaskId { get; init; }
    public int ListId { get; init; }
    public bool NotifySignalR { get; init; }

    public string TaskName { get; init; } = null!;
    public string ListName { get; init; } = null!;
    public string ActionUserName { get; init; } = null!;
    public string ActionUserImageUri { get; init; } = null!;
    public IReadOnlyList<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
