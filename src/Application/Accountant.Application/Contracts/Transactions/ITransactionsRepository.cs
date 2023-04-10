using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Transactions;

public interface ITransactionsRepository
{
    IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized);
}
