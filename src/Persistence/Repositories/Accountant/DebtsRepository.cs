using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class DebtsRepository : BaseRepository, IDebtsRepository
    {
        public DebtsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<Debt>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            return await Dapper.QueryAsync<Debt>(@"SELECT * FROM ""Accountant.Debts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            return await Dapper.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Debt, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Debt debt, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                id = (await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.Debts"" (""UserId"", ""Person"", ""Amount"", ""Currency"", ""Description"", ""UserIsDebtor"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @Person, @Amount, @Currency, @Description, @UserIsDebtor, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       debt)).Single();
            }
            else
            {
                id = (await uowConn.QueryAsync<int>(@"INSERT INTO ""Accountant.Debts"" (""UserId"", ""Person"", ""Amount"", ""Currency"", ""Description"", ""UserIsDebtor"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @Person, @Amount, @Currency, @Description, @UserIsDebtor, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       debt, uowTransaction)).Single();
            }

            return id;
        }

        public async Task UpdateAsync(Debt debt)
        {
            await Dapper.ExecuteAsync(@"UPDATE ""Accountant.Debts"" SET ""Person"" = @Person, ""Amount"" = @Amount, 
                                    ""Currency"" = @Currency, ""Description"" = @Description, ""UserIsDebtor"" = @UserIsDebtor, 
                                    ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", debt);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var transaction = Dapper.BeginTransaction();

            var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id });

            if (deletedEntryExists)
            {
                await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.Debts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
