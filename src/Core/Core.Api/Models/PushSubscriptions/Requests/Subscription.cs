namespace Core.Api.Models.PushNotifications.Requests;

public record Subscription(string Endpoint, Dictionary<string, string> Keys);
