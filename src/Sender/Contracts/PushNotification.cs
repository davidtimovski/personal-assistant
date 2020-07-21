﻿namespace PersonalAssistant.Sender.Contracts
{
    internal class PushNotification
    {
        public string SenderImageUri { get; set; }
        public int UserId { get; set; }
        public string Application { get; set; }
        public string Message { get; set; }
        public string OpenUrl { get; set; }
    }
}
