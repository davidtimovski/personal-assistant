namespace Sender.Contracts;

internal class PushNotification
{
    internal string Application { get; set; }
    internal string SenderImageUri { get; set; }
    internal int UserId { get; set; }
    internal string Message { get; set; }
    internal string OpenUrl { get; set; }
}
