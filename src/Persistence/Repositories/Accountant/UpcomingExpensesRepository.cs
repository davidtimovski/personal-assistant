using System;
using System.Collections.Generic;
using System.Data.Common;
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
            return await Dapper.QueryAsync<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            return await Dapper.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, DeletedDate = fromDate });
        }

        public async Task<bool> ExistsAsync(int categoryId, DateTime now)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""Accountant.UpcomingExpenses""
                                                        WHERE ""CategoryId"" = @CategoryId AND ""Generated"" 
                                                        AND to_char(""CreatedDate"", 'YYYY-MM') = to_char(@Now, 'YYYY-MM')",
                                                        new { CategoryId = categoryId, Now = now });
        }

        public async Task<int> CreateAsync(UpcomingExpense upcomingExpense, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                id = (await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.UpcomingExpenses"" (""UserId"", ""CategoryId"", ""Amount"", ""Currency"", ""Description"", ""Date"", ""Generated"", ""CreatedDate"", ""ModifiedDate"")
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
            await Dapper.ExecuteAsync(@"UPDATE ""Accountant.UpcomingExpenses"" SET ""CategoryId"" = @CategoryId, ""Amount"" = @Amount, 
                                        ""Currency"" = @Currency, ""Description"" = @Description, ""Date"" = @Date, 
                                        ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", upcomingExpense);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var transaction = Dapper.BeginTransaction();

            var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id });

            if (deletedEntryExists)
            {
                await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }

        public async Task DeleteOldAsync(int userId, DateTime before)
        {
            var transaction = Dapper.BeginTransaction();

            var toDelete = await Dapper.QueryAsync<UpcomingExpense>(@"SELECT * FROM ""Accountant.UpcomingExpenses"" WHERE ""UserId"" = @UserId AND ""Date"" < @Date",
                new { UserId = userId, Date = before });

            foreach (var upcomingExpense in toDelete)
            {
                var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                            FROM ""Accountant.DeletedEntities""
                                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                                            new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id });

                if (deletedEntryExists)
                {
                    await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                                 WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                 new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                                 transaction);
                }
                else
                {
                    await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                                 VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                                 new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                                                 transaction);
                }

                await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.UpcomingExpenses"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                    new { upcomingExpense.Id, UserId = userId }, transaction);
            }

            transaction.Commit();
        }
    }
}
