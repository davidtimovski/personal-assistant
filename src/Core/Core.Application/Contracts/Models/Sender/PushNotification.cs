namespace Core.Application.Contracts.Models.Sender;

public class PushNotification : ISendable
{
    protected PushNotification(string application)
    {
        Application = application;
    }

    public string Application { get; init; }
    public required string SenderImageUri { get; init; }
    public required int UserId { get; init; }
    public required string Message { get; init; }
    public string? OpenUrl { get; init; }
}
