using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class AccountsRepository : BaseRepository, IAccountsRepository
    {
        public AccountsRepository(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task<IEnumerable<Account>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Account>(@"SELECT * FROM ""Accountant.Accounts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Account, DeletedDate = fromDate });
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> IsMainAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId AND ""IsMain""",
                new { Id = id, UserId = userId });
        }

        public async Task<int> CreateAsync(Account account, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                using DbConnection conn = Connection;
                await conn.OpenAsync();

                id = (await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.Accounts"" (""UserId"", ""Name"", ""IsMain"", ""Currency"", ""StockPrice"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @Name, @IsMain, @Currency, @StockPrice, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       account)).Single();
            }
            else
            {
                id = (await uowConn.QueryAsync<int>(@"INSERT INTO ""Accountant.Accounts"" (""UserId"", ""Name"", ""IsMain"", ""Currency"", ""StockPrice"", ""CreatedDate"", ""ModifiedDate"")
                                                   VALUES (@UserId, @Name, @IsMain, @Currency, @StockPrice, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                       account, uowTransaction)).Single();
            }

            return id;
        }

        public async Task UpdateAsync(Account account)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"UPDATE ""Accountant.Accounts"" SET ""Name"" = @Name, ""Currency"" = @Currency, ""StockPrice"" = @StockPrice, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", account);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
