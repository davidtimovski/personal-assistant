using Accountant.Application.Contracts.Accounts.Models;
using Accountant.Application.Contracts.Common.Models;

namespace Accountant.Application.Contracts.Accounts;

public interface IAccountService
{
    IEnumerable<AccountDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateAccount model);
    Task CreateMainAsync(CreateMainAccount model);
    Task UpdateAsync(UpdateAccount model);
    Task DeleteAsync(int id, int userId);
}
