using System;
using System.Collections.Generic;
using System.Data.Common;
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
            return await Dapper.QueryAsync<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""GenerateUpcomingExpense""");
        }

        public async Task<IEnumerable<Category>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            return await Dapper.QueryAsync<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            return await Dapper.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Category, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Category category, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                id = (await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.Categories"" (""ParentId"", ""UserId"", ""Name"", ""Type"", ""GenerateUpcomingExpense"",""CreatedDate"", ""ModifiedDate"")
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
            await Dapper.ExecuteAsync(@"UPDATE ""Accountant.Categories"" SET ""ParentId"" = @ParentId, ""Name"" = @Name, ""Type"" = @Type, 
                ""GenerateUpcomingExpense"" = @GenerateUpcomingExpense, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", category);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var transaction = Dapper.BeginTransaction();

            var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id });

            if (deletedEntryExists)
            {
                await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Category, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.Categories"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
