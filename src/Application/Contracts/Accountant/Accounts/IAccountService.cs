using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.Common.Models;

namespace Application.Contracts.Accountant.Accounts
{
    public interface IAccountService
    {
        IEnumerable<AccountDto> GetAll(GetAll model);
        IEnumerable<int> GetDeletedIds(GetDeletedIds model);
        Task<int> CreateAsync(CreateAccount model);
        Task CreateMainAsync(CreateMainAccount model);
        Task UpdateAsync(UpdateAccount model);
        Task DeleteAsync(int id, int userId);
    }
}
