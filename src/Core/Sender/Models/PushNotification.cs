using System.Text.Json.Serialization;

namespace Sender.Models;

public class PushNotification
{
    [JsonRequired]
    public string Application { get; set; } = null!;

    [JsonRequired]
    public string SenderImageUri { get; set; } = null!;

    [JsonRequired]
    public int UserId { get; set; }

    [JsonRequired]
    public string Message { get; set; } = null!;

    [JsonRequired]
    public string OpenUrl { get; set; } = null!;
}
