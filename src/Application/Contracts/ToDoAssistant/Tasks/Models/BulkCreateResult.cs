using System.Collections.Generic;
using System.Linq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class BulkCreateResult : INotificationResult
    {
        public BulkCreateResult(int listId, bool notifySignalR)
        {
            ListId = listId;
            NotifySignalR = notifySignalR;
        }

        public int ListId { get; }
        public bool NotifySignalR { get; }

        public string ListName { get; set; }
        public IEnumerable<BulkCreatedTask> CreatedTasks { get; set; }
        public string ActionUserImageUri { get; set; }
        public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

        public bool Notify()
        {
            return NotificationRecipients.Any() && CreatedTasks.Any();
        }
    }

    public class BulkCreatedTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
