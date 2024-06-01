public class CreateFriendship
{
    public required int UserId { get; init; }
    public required string Email { get; init; }
    public required IReadOnlySet<string> Permissions { get; init; }
}
