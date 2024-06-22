namespace ToDoAssistant.Api.Models.Lists.Responses;

public class CanShareResponse
{
    public CanShareResponse(bool canShare)
    {
        CanShare = canShare;
    }

    public CanShareResponse(bool canShare, int userId, string imageUri)
    {
        CanShare = canShare;
        UserId = userId;
        ImageUri = imageUri;
    }


    public bool CanShare { get; }
    public int UserId { get; }
    public string? ImageUri { get; }
}
