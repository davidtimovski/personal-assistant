﻿using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class LeaveListResult : INotificationResult, IResult
{
    public LeaveListResult()
    {
        Failed = true;
    }

    public LeaveListResult(bool success)
    {
        Failed = !success;
    }

    public bool Failed { get; private set; }

    public string ListName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyList<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
