namespace Sender.Models;

internal class PushNotification
{
    public string Application { get; set; } = null!;
    public string SenderImageUri { get; set; } = null!;
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public string OpenUrl { get; set; } = null!;
}
