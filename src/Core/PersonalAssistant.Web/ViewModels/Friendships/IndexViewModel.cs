namespace PersonalAssistant.Web.ViewModels.Friendships;

public enum FriendshipsIndexAlert
{
    None,
    FriendshipRequestSent,
    FriendshipUpdated,
    FriendshipAccepted,
    FriendshipDeclined,
    FriendshipDeleted
}

public class IndexViewModel
{
    public required FriendshipsIndexAlert Alert { get; init; }
    public required IReadOnlyList<FriendshipItemViewModel> Friendships { get; init; }
}

public class FriendshipItemViewModel
{
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string ImageUri { get; init; }
    public required bool? IsAccepted { get; init; }
    public required bool UserIsSender { get; init; }
}
