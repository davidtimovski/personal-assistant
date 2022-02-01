namespace Infrastructure.Sender.Models
{
    public abstract class PushNotification
    {
        public PushNotification(string application)
        {
            Application = application;
        }

        public string Application { get; }
        public string SenderImageUri { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public string OpenUrl { get; set; }
    }

    public class ToDoAssistantPushNotification : PushNotification
    {
        public ToDoAssistantPushNotification() : base("To Do Assistant") { }
    }

    public class CookingAssistantPushNotification : PushNotification
    {
        public CookingAssistantPushNotification() : base("Cooking Assistant") { }
    }
}
