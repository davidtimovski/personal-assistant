using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

public class CreateUserPayload
{
    public CreateUserPayload(string email, bool email_verified, string password, string name)
    {
        this.email = email;
        this.email_verified = email_verified;
        this.password = password;
        this.name = name;
    }

    [JsonRequired]
    public string email { get; init; }

    [JsonRequired]
    public bool email_verified { get; init; }

    [JsonRequired]
    public string password { get; init; }

    [JsonRequired]
    public string name { get; init; }

    [JsonRequired]
    public string connection { get; init; } = "Username-Password-Authentication";
}

public class CreateUserResult
{
    [JsonRequired]
    public required string user_id { get; init; }
}
