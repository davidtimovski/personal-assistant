namespace Core.Application.Contracts.Models;

public class FriendshipDto
{
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string ImageUri { get; init; }
    public required IReadOnlySet<string> Permissions { get; init; }
    public required bool? IsAccepted { get; init; }
    public required bool UserIsSender { get; init; }
}
