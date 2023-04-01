using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.AutomaticTransactions;

public interface IAutomaticTransactionsRepository
{
    Task<int> CreateAsync(AutomaticTransaction automaticTransaction);
    Task UpdateAsync(AutomaticTransaction automaticTransaction);
    Task DeleteAsync(int id, int userId);
}
