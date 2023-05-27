namespace Core.Infrastructure.Identity;

public class Auth0Exception : Exception
{
    public Auth0Exception(string message) : base(message)
    {
    }
}
