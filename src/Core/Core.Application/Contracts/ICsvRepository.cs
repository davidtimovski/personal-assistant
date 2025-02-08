using Core.Application.Contracts.Models;

namespace Core.Application.Contracts;

public interface ICsvRepository
{
    List<TransactionForExport> GetAllTransactionsForExport(int userId, string uncategorized, ISpan metricsSpan);
}
