using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Application.Contracts;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int? GetId(string auth0Id);
    bool Exists(string email);
    bool Exists(int id);
    Task<int> CreateAsync(string auth0Id, User user, ISpan metricsSpan);
    Task UpdateAsync(User user, ISpan metricsSpan);
    Task DeleteAsync(int id, ISpan metricsSpan);
}
