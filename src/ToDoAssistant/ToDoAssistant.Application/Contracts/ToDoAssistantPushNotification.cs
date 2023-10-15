using Core.Application.Contracts.Models.Sender;

namespace ToDoAssistant.Application.Contracts;

public class ToDoAssistantPushNotification : PushNotification
{
    public ToDoAssistantPushNotification() : base("To Do Assistant") { }
}
