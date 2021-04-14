using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts
{
    public interface IAccountsRepository
    {
        Task<IEnumerable<Account>> GetAllAsync(int userId, DateTime fromModifiedDate);
        Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> IsMainAsync(int id, int userId);
        Task<int> CreateAsync(Account account, IDbConnection uowConn = null, IDbTransaction uowTransaction = null);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int id, int userId);
    }
}
