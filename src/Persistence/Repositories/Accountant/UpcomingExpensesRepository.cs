using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class UpcomingExpensesRepository : BaseRepository, IUpcomingExpensesRepository
    {
        public UpcomingExpensesRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<UpcomingExpense>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, DeletedDate = fromDate });
        }

        public async Task<bool> ExistsAsync(int categoryId, DateTime now)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""Accountant.UpcomingExpenses""
                                                        WHERE ""CategoryId"" = @CategoryId AND ""Generated"" 
                                                        AND to_char(""CreatedDate"", 'YYYY-MM') = to_char(@Now, 'YYYY-MM')",
                                                        new { CategoryId = categoryId, Now = now });
        }

        public async Task<int> CreateAsync(UpcomingExpense upcomingExpense, IDbConnection uowConn = null, IDbTransaction uowTransaction = null)
        {
            using IDbConnection conn = OpenConnection();

            int id;

            if (uowConn == null && uowTransaction == null)
            {
                id = (await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.UpcomingExpenses"" (""UserId"", ""CategoryId"", ""Amount"", ""Currency"", ""Description"", ""Date"", ""Generated"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @CategoryId, @Amount, @Currency, @Description, @Date, @Generated, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       upcomingExpense)).Single();
            }
            else
            {
                id = (await uowConn.QueryAsync<int>(@"INSERT INTO ""Accountant.UpcomingExpenses"" (""UserId"", ""CategoryId"", ""Amount"", ""Currency"", ""Description"", ""Date"", ""Generated"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @CategoryId, @Amount, @Currency, @Description, @Date, @Generated, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       upcomingExpense, uowTransaction)).Single();
            }

            return id;
        }

        public async Task UpdateAsync(UpcomingExpense upcomingExpense)
        {
            UpcomingExpense dbUpcomingExpense = EFContext.UpcomingExpenses.Find(upcomingExpense.Id);

            dbUpcomingExpense.CategoryId = upcomingExpense.CategoryId;
            dbUpcomingExpense.Amount = upcomingExpense.Amount;
            dbUpcomingExpense.Currency = upcomingExpense.Currency;
            dbUpcomingExpense.Description = upcomingExpense.Description;
            dbUpcomingExpense.Date = upcomingExpense.Date;
            dbUpcomingExpense.ModifiedDate = upcomingExpense.ModifiedDate;

            await EFContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }

        public async Task DeleteOldAsync(int userId, DateTime before)
        {
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            var toDelete = await conn.QueryAsync<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses"" WHERE ""UserId"" = @UserId AND ""Date"" < @Date",
                new { UserId = userId, Date = before });

            foreach (var upcomingExpense in toDelete)
            {
                var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                            FROM ""Accountant.DeletedEntities""
                                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                                            new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id });

                if (deletedEntryExists)
                {
                    await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                                 WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                 new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                                 transaction);
                }
                else
                {
                    await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                                 VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                                 new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                                 transaction);
                }

                await conn.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                    new { upcomingExpense.Id, UserId = userId }, transaction);
            }

            transaction.Commit();
        }
    }
}
