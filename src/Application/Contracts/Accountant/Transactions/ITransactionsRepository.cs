using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Transactions;

public interface ITransactionsRepository
{
    IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized);
    IEnumerable<Transaction> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(int id, int userId);
}
