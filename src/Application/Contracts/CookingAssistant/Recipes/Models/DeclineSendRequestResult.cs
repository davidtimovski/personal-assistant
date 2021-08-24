using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class DeclineSendRequestResult : INotificationResult
    {
        public string RecipeName { get; set; }
        public string ActionUserImageUri { get; set; }
        public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; }

        public bool Notify()
        {
            return true;
        }
    }
}
