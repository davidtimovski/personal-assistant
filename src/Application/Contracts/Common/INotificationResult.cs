﻿using System.Collections.Generic;
using Application.Contracts.Common.Models;

namespace Application.Contracts.Common;

public interface INotificationResult
{
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; }

    public bool Notify();
}
