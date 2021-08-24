using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface INotificationResult
    {
        public string ActionUserImageUri { get; set; }
        public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; }

        public bool Notify();
    }
}
