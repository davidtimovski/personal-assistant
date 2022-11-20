using Domain.Accountant;

namespace Accountant.Application.Contracts.AutomaticTransactions;

public interface IAutomaticTransactionsRepository
{
    IEnumerable<AutomaticTransaction> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(AutomaticTransaction automaticTransaction);
    Task UpdateAsync(AutomaticTransaction automaticTransaction);
    Task DeleteAsync(int id, int userId);
}
