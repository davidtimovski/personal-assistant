using Application.Domain.Common;

namespace Core.Application.Contracts;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int? GetId(string auth0Id);
    bool Exists(string email);
    bool Exists(int id);
    Task<int> CreateAsync(string auth0Id, User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
