using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class DebtsRepository : BaseRepository, IDebtsRepository
    {
        public DebtsRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<IEnumerable<Debt>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Debt>(@"SELECT * FROM ""Accountant.Debts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Debt, DeletedDate = fromDate });
        }

        public async Task<int> CreateAsync(Debt debt, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                using DbConnection conn = Connection;
                await conn.OpenAsync();

                id = (await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.Debts"" (""UserId"", ""Person"", ""Amount"", ""Currency"", ""Description"", ""UserIsDebtor"", ""CreatedDate"", ""ModifiedDate"")
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
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"UPDATE ""Accountant.Debts"" SET ""Person"" = @Person, ""Amount"" = @Amount, 
                                    ""Currency"" = @Currency, ""Description"" = @Description, ""UserIsDebtor"" = @UserIsDebtor, 
                                    ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", debt);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Debt, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.Debts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
