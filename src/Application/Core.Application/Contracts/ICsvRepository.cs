using Application.Domain.Accountant;

namespace Core.Application.Contracts;

public interface ICsvRepository
{
    IEnumerable<Transaction> GetAllTransactionsForExport(int userId, string uncategorized);
}
