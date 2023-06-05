namespace Sender.Models;

public class PushNotificationMessage
{
    public PushNotificationMessage(string senderImageUri, string title, string message, string openUrl)
    {
        SenderImageUri = senderImageUri;
        Title = title;
        Body = TrimMessagePlaceholders(message);
        OpenUrl = openUrl;
    }

    public string SenderImageUri { get; }
    public string Title { get; }
    public string Body { get; }
    public string OpenUrl { get; }

    private static string TrimMessagePlaceholders(string message)
    {
        return message.Replace("#[", string.Empty, StringComparison.Ordinal).Replace("]#", string.Empty, StringComparison.Ordinal);
    }
}
