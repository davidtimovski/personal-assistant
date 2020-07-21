namespace PersonalAssistant.Infrastructure.Sender.Models
{
    public class PushNotification
    {
        public string SenderImageUri { get; set; }
        public int UserId { get; set; }
        public string Application { get; set; }
        public string Message { get; set; }
        public string OpenUrl { get; set; }
    }
}
