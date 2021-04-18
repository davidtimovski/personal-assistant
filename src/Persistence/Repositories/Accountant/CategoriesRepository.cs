using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class CategoriesRepository : BaseRepository, ICategoriesRepository
    {
        public CategoriesRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<Category>> GetAllWithGenerateAsync()
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""GenerateUpcomingExpense""");
        }

        public async Task<IEnumerable<Category>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Category, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Category category, IDbConnection uowConn = null, IDbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                using IDbConnection conn = OpenConnection();

                id = (await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.Categories"" (""ParentId"", ""UserId"", ""Name"", ""Type"", ""GenerateUpcomingExpense"",""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@ParentId, @UserId, @Name, @Type, @GenerateUpcomingExpense, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       category)).Single();
            }
            else
            {
                id = (await uowConn.QueryAsync<int>(@"INSERT INTO ""Accountant.Categories"" (""ParentId"", ""UserId"", ""Name"", ""Type"", ""GenerateUpcomingExpense"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@ParentId, @UserId, @Name, @Type, @GenerateUpcomingExpense, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       category, uowTransaction)).Single();
            }

            return id;
        }

        public async Task UpdateAsync(Category category)
        {
            Category dbCategory = EFContext.Categories.Find(category.Id);

            dbCategory.ParentId = category.ParentId;
            dbCategory.Name = category.Name;
            dbCategory.Type = category.Type;
            dbCategory.GenerateUpcomingExpense = category.GenerateUpcomingExpense;
            dbCategory.ModifiedDate = category.ModifiedDate;

            await EFContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.Categories"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
