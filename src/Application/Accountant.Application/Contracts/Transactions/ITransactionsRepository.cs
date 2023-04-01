using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Transactions;

public interface ITransactionsRepository
{
    IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized);
    Task<int> CreateAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(int id, int userId);
}
