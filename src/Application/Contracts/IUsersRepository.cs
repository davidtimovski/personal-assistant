using Domain.Common;

namespace Application.Contracts;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int? GetId(string auth0Id);
    bool Exists(int id);
    Task UpdateAsync(User user);
}
