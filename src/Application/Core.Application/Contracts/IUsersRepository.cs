using Application.Domain.Common;
using Sentry;
using User = Application.Domain.Common.User;

namespace Core.Application.Contracts;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int? GetId(string auth0Id);
    bool Exists(string email);
    bool Exists(int id);
    Task<int> CreateAsync(string auth0Id, User user, ITransaction tr);
    Task UpdateAsync(User user, ITransaction tr);
    Task DeleteAsync(int id, ITransaction tr);
}
