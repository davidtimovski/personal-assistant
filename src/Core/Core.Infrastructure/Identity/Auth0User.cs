using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

public class Auth0User
{
    [JsonRequired]
    public string email { get; set; } = null!;

    [JsonRequired]
    public string name { get; set; } = null!;
}
