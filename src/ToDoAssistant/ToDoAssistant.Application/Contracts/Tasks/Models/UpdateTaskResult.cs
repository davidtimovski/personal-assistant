using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation.Results;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class UpdateTaskResult : INotificationResult, IValidatedResult
{
    public UpdateTaskResult(ResultStatus status)
    {
        if (status == ResultStatus.Invalid)
        {
            throw new ArgumentException($"Use this constructor only with {nameof(ResultStatus.Successful)} and {nameof(ResultStatus.Error)} status");
        }

        Status = status;
    }

    public UpdateTaskResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyList<ValidationFailure>? ValidationErrors { get; private set; }

    public string? OriginalTaskName { get; init; }
    public int ListId { get; init; }
    public string? ListName { get; init; }
    public bool NotifySignalR { get; init; }

    public int? OldListId { get; set; }
    public string? OldListName { get; set; }
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyList<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
    public IReadOnlyList<NotificationRecipient> RemovedNotificationRecipients { get; set; } = new List<NotificationRecipient>();
    public IReadOnlyList<NotificationRecipient> CreatedNotificationRecipients { get; set; } = new List<NotificationRecipient>();
    public NotificationRecipient? AssignedNotificationRecipient { get; set; }

    public bool Notify()
    {
        return NotificationRecipients.Any() || RemovedNotificationRecipients.Any() || CreatedNotificationRecipients.Any() || AssignedNotificationRecipient is not null;
    }
}
