using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation.Results;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class UpdateListResult : INotificationResult, IValidatedResult
{
    public UpdateListResult(ResultStatus status)
    {
        if (status == ResultStatus.Invalid)
        {
            throw new ArgumentException($"Use this constructor only with {nameof(ResultStatus.Successful)} and {nameof(ResultStatus.Error)} status");
        }

        Status = status;
    }

    public UpdateListResult(List<ValidationFailure> validationErrors)
    {
        Status = ResultStatus.Invalid;
        ValidationErrors = validationErrors;
    }

    public ResultStatus Status { get; private set; }
    public IReadOnlyCollection<ValidationFailure>? ValidationErrors { get; private set; }

    public ListNotificationType Type { get; set; }
    public string OriginalListName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
