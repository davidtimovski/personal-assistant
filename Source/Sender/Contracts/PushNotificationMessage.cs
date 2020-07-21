using System;

namespace PersonalAssistant.Sender.Contracts
{
    internal class PushNotificationMessage
    {
        internal PushNotificationMessage(string senderImageUri, string title, string message, string openUrl)
        {
            SenderImageUri = senderImageUri;
            Title = title;
            Body = TrimMessagePlaceholders(message);
            OpenUrl = openUrl;
        }

        public string SenderImageUri { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string OpenUrl { get; set; }

        private static string TrimMessagePlaceholders(string message)
        {
            return message.Replace("#[", string.Empty, StringComparison.Ordinal).Replace("]#", string.Empty, StringComparison.Ordinal);
        }
    }
}
