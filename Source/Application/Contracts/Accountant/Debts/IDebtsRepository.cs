﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Debts
{
    public interface IDebtsRepository
    {
        Task<IEnumerable<Debt>> GetAllAsync(int userId, DateTime fromModifiedDate);
        Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate);
        Task<int> CreateAsync(Debt debt, DbConnection uowConn = null, DbTransaction uowTransaction = null);
        Task UpdateAsync(Debt debt);
        Task DeleteAsync(int id, int userId);
    }
}
