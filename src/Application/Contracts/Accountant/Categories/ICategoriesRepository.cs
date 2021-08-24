﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Categories
{
    public interface ICategoriesRepository
    {
        IEnumerable<Category> GetAll(int userId, DateTime fromModifiedDate);
        IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
        Task<int> CreateAsync(Category category, IDbConnection uowConn = null, IDbTransaction uowTransaction = null);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id, int userId);
    }
}
