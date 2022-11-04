using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Application.Contracts.Common;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int? GetId(string auth0Id);
    bool Exists(int id);
    Task UpdateAsync(User user);
}
