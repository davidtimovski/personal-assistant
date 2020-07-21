﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Accountant.Transactions
{
    public interface ITransactionsRepository
    {
        Task<IEnumerable<Transaction>> GetAllForExportAsync(int userId, string uncategorized);
        Task<IEnumerable<Transaction>> GetAllAsync(int userId, DateTime fromModifiedDate);
        Task<IEnumerable<Transaction>> GetAllAsync(int userId, int categoryId, DateTime from, DateTime to);
        Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate);
        Task<int> CreateAsync(Transaction transaction, DbConnection uowConn = null, DbTransaction uowTransaction = null);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id, int userId);
    }
}
