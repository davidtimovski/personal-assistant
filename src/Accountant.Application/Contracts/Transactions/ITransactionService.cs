using Accountant.Application.Contracts.Transactions.Models;

namespace Accountant.Application.Contracts.Transactions;

public interface ITransactionService
{
    Task<int> CreateAsync(CreateTransaction model);
    Task UpdateAsync(UpdateTransaction model);
    FileStream ExportAsCsv(ExportAsCsv model);
}
