namespace Core.Api.Models.PushNotifications.Requests;

public record PushNotificationsSubscriptionRequest(string Application, Subscription Subscription);
