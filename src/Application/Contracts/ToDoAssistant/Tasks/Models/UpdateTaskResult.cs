﻿using System.Collections.Generic;
using System.Linq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class UpdateTaskResult : INotificationResult
    {
        public UpdateTaskResult(string originalTaskName, int listId, string listName, bool notifySignalR)
        {
            OriginalTaskName = originalTaskName;
            ListId = listId;
            ListName = listName;
            NotifySignalR = notifySignalR;
        }

        public string OriginalTaskName { get; }
        public int ListId { get; }
        public string ListName { get; }
        public bool NotifySignalR { get; }

        public int OldListId { get; set; }
        public string OldListName { get; set; }
        public string ActionUserImageUri { get; set; }
        public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
        public IEnumerable<NotificationRecipient> RemovedNotificationRecipients { get; set; } = new List<NotificationRecipient>();
        public IEnumerable<NotificationRecipient> CreatedNotificationRecipients { get; set; } = new List<NotificationRecipient>();
        public NotificationRecipient AssignedNotificationRecipient { get; set; }

        public bool Notify()
        {
            return NotificationRecipients.Any() || RemovedNotificationRecipients.Any() || CreatedNotificationRecipients.Any() || AssignedNotificationRecipient != null;
        }
    }
}
