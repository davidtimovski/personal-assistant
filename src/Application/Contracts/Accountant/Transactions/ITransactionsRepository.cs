﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Accountant.Transactions
{
    public interface ITransactionsRepository
    {
        IEnumerable<Transaction> GetAllForExport(int userId, string uncategorized);
        IEnumerable<Transaction> GetAll(int userId, DateTime fromModifiedDate);
        IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
        Task<int> CreateAsync(Transaction transaction, IDbConnection uowConn = null, IDbTransaction uowTransaction = null);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id, int userId);
    }
}
