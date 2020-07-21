using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAllAsync(GetAll model);
        Task<IEnumerable<int>> GetDeletedIdsAsync(GetDeletedIds model);
        Task<int> CreateAsync(CreateAccount model);
        Task CreateMainAsync(CreateMainAccount model);
        Task UpdateAsync(UpdateAccount model);
        Task DeleteAsync(int id, int userId);
    }
}
