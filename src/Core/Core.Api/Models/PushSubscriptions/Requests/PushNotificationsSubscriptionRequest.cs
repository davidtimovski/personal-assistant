using System.ComponentModel.DataAnnotations;

namespace Core.Api.Models.PushNotifications.Requests;

public record PushNotificationsSubscriptionRequest([Required] string Application, [Required] Subscription Subscription);
