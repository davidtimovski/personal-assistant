using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

internal class CreateUserPayload
{
    public CreateUserPayload(string email, bool email_verified, string password, string name)
    {
        this.email = email;
        this.email_verified = email_verified;
        this.password = password;
        this.name = name;
    }

    [JsonRequired]
    public string email { get; set; }

    [JsonRequired]
    public bool email_verified { get; set; }

    [JsonRequired]
    public string password { get; set; }

    [JsonRequired]
    public string name { get; set; }

    [JsonRequired]
    public string connection { get; } = "Username-Password-Authentication";
}

internal class CreateUserResult
{
    [JsonRequired]
    public string user_id { get; set; } = null!;
}
