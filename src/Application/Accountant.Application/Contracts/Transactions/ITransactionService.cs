using Accountant.Application.Contracts.Transactions.Models;

namespace Accountant.Application.Contracts.Transactions;

public interface ITransactionService
{
    FileStream ExportAsCsv(ExportAsCsv model);
}
