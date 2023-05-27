namespace Core.Application.Contracts;

public interface IUserIdLookup
{
    bool Contains(string auth0Id);
    int Get(string auth0Id);
    void Set(string auth0Id, int userId);
}
