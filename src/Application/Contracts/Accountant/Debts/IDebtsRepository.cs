using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Debts;

public interface IDebtsRepository
{
    IEnumerable<Debt> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(Debt debt);
    Task<int> CreateMergedAsync(Debt debt);
    Task UpdateAsync(Debt debt);
    Task DeleteAsync(int id, int userId);
}
