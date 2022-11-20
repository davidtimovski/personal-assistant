﻿using Application.Contracts.Models;

namespace Application.Contracts;

public interface INotificationResult
{
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; }

    public bool Notify();
}