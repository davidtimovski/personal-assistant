using System.Collections.Concurrent;
using Application.Contracts;

namespace Infrastructure.Identity;

public class UserIdLookup : IUserIdLookup
{
    private readonly ConcurrentDictionary<string, int> _userIdsLookup = new();

    public bool Contains(string auth0Id)
    {
        return _userIdsLookup.ContainsKey(auth0Id);
    }

    public int Get(string auth0Id)
    {
        return _userIdsLookup[auth0Id];
    }

    public void Set(string auth0Id, int userId)
    {
        _userIdsLookup.AddOrUpdate(auth0Id, userId, (key, oldValue) => userId);
    }
}
