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

    public string email { get; set; }
    public bool email_verified { get; set; }
    public string password { get; set; }
    public string name { get; set; }
    public string connection { get; } = "Username-Password-Authentication";
}

internal class CreateUserResult
{
    public string user_id { get; set; }
}
