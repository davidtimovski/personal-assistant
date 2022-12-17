internal class CreateUserPayload
{
    public CreateUserPayload(string email, bool email_verified, string password, string name, string connection)
    {
        this.email = email;
        this.email_verified = email_verified;
        this.password = password;
        this.name = name;
        this.connection = connection;
    }

    public string email { get; set; }
    public bool email_verified { get; set; }
    public string password { get; set; }
    public string name { get; set; }
    public string connection { get; set; }
}

internal class CreateUserResult
{
    public string user_id { get; set; }
}
