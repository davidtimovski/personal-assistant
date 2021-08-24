using System.Collections.Generic;
using System.Linq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class CreatedTaskResult : INotificationResult
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string ListName { get; set; }
        public string ActionUserImageUri { get; set; }
        public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

        public bool Notify()
        {
            return NotificationRecipients.Any();
        }
    }
}
