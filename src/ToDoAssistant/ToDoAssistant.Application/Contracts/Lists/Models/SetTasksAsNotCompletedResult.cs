﻿using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class SetTasksAsNotCompletedResult : INotificationResult
{
    public string ListName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public List<NotificationRecipient> NotificationRecipients { get; set; } = new();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
