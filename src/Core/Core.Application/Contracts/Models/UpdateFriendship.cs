public class UpdateFriendship
{
    public required int UserId { get; init; }
    public required int FriendId { get; init; }
    public required IReadOnlySet<string> Permissions { get; init; }
}
