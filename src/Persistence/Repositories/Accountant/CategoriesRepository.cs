﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Categories;
using Dapper;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant;

public class CategoriesRepository : BaseRepository, ICategoriesRepository
{
    public CategoriesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Category> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Category, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(Category category)
    {
        EFContext.Categories.Add(category);
        await EFContext.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(Category category)
    {
        Category dbCategory = EFContext.Categories.Find(category.Id);

        dbCategory.ParentId = category.ParentId;
        dbCategory.Name = category.Name;
        dbCategory.Type = category.Type;
        dbCategory.GenerateUpcomingExpense = category.GenerateUpcomingExpense;
        dbCategory.IsTax = category.IsTax;
        dbCategory.ModifiedDate = category.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.Category && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.Category,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var category = EFContext.Categories.First(x => x.Id == id && x.UserId == userId);
        EFContext.Categories.Remove(category);

        await EFContext.SaveChangesAsync();
    }
}
