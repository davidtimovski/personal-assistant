using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

public class Auth0User
{
    [JsonRequired]
    public required string email { get; init; }

    [JsonRequired]
    public required string name { get; init; }
}
