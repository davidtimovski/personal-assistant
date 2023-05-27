﻿namespace Core.Application.Contracts.Models.Sender;

public class PushNotification : ISendable
{
    protected PushNotification(string application)
    {
        Application = application;
    }

    public string Application { get; } = null!;
    public string SenderImageUri { get; set; } = null!;
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public string OpenUrl { get; set; } = null!;
}

public class ToDoAssistantPushNotification : PushNotification
{
    public ToDoAssistantPushNotification() : base("To Do Assistant") { }
}

public class CookingAssistantPushNotification : PushNotification
{
    public CookingAssistantPushNotification() : base("Cooking Assistant") { }
}