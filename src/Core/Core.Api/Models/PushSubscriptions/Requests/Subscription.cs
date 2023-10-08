using System.ComponentModel.DataAnnotations;

namespace Core.Api.Models.PushNotifications.Requests;

public record Subscription([Required] string Endpoint, [Required] Dictionary<string, string> Keys);
