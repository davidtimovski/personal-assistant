namespace PersonalAssistant.Web.ViewModels.Friendships;

public class ViewFriendshipViewModel
{
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string ImageUri { get; init; }
    public required IReadOnlySet<string> Permissions { get; set; }
    public required bool? IsAccepted { get; set; }
    public required bool UserIsSender { get; init; }
}
