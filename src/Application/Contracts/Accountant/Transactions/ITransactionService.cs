using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Transactions
{
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
}
