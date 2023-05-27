namespace Sender.Models;

internal class PushNotificationMessage
{
    internal PushNotificationMessage(string senderImageUri, string title, string message, string openUrl)
    {
        SenderImageUri = senderImageUri;
        Title = title;
        Body = TrimMessagePlaceholders(message);
        OpenUrl = openUrl;
    }

    internal string SenderImageUri { get; }
    internal string Title { get; }
    internal string Body { get; }
    internal string OpenUrl { get; }

    private static string TrimMessagePlaceholders(string message)
    {
        return message.Replace("#[", string.Empty, StringComparison.Ordinal).Replace("]#", string.Empty, StringComparison.Ordinal);
    }
}
