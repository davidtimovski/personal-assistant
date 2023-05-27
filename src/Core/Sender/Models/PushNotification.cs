namespace Sender.Models;

internal class PushNotification
{
    public string Application { get; set; }
    public string SenderImageUri { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public string OpenUrl { get; set; }
}
