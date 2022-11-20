using Accountant.Application.Contracts.Common.Models;
using Accountant.Application.Contracts.Transactions.Models;

namespace Accountant.Application.Contracts.Transactions;

public interface ITransactionService
{
    IEnumerable<TransactionDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateTransaction model);
    Task UpdateAsync(UpdateTransaction model);
    Task DeleteAsync(int id, int userId);
    FileStream ExportAsCsv(ExportAsCsv model);
    void DeleteExportedFile(DeleteExportedFile model);
}
