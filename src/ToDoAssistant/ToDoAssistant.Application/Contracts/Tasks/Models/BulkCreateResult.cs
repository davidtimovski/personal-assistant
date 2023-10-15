using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation.Results;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class BulkCreateResult : INotificationResult, IValidatedResult
{
    public BulkCreateResult(ResultStatus status)
    {
        if (status == ResultStatus.Invalid)
        {
            throw new ArgumentException($"Use this constructor only with {nameof(ResultStatus.Successful)} and {nameof(ResultStatus.Error)} status");
        }

        Status = status;
    }

    public BulkCreateResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyCollection<ValidationFailure>? ValidationErrors { get; private set; }

    public int ListId { get; init; }
    public bool NotifySignalR { get; init; }

    public string ListName { get; set; } = null!;
    public IReadOnlyCollection<BulkCreatedTask> CreatedTasks { get; set; } = new List<BulkCreatedTask>();
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any() && CreatedTasks.Any();
    }
}

public class BulkCreatedTask
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
