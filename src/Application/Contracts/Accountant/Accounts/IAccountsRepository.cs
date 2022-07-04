﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Accounts;

public interface IAccountsRepository
{
    IEnumerable<Account> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    bool Exists(int id, int userId);
    bool HasMain(int userId);
    bool IsMain(int id, int userId);
    Task<int> CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id, int userId);
}
