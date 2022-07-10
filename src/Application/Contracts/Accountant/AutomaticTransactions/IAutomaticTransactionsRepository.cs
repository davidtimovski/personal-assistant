using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.AutomaticTransactions;

public interface IAutomaticTransactionsRepository
{
    IEnumerable<AutomaticTransaction> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(AutomaticTransaction automaticTransaction);
    Task UpdateAsync(AutomaticTransaction automaticTransaction);
    Task DeleteAsync(int id, int userId);
}
