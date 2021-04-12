using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Persistence.Repositories.Accountant
{
    public class AccountsRepository : BaseRepository, IAccountsRepository
    {
        public AccountsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<Account>> GetAllAsync(int userId, DateTime fromModifiedDate)
        {
            return await Dapper.QueryAsync<Account>(@"SELECT * FROM ""Accountant.Accounts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
                new { UserId = userId, FromModifiedDate = fromModifiedDate });
        }

        public async Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate)
        {
            return await Dapper.QueryAsync<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
                new { UserId = userId, EntityType = (short)EntityType.Account, DeletedDate = fromDate });
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> IsMainAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId AND ""IsMain""",
                new { Id = id, UserId = userId });
        }

        public async Task<int> CreateAsync(Account account, DbConnection uowConn = null, DbTransaction uowTransaction = null)
        {
            int id;

            if (uowConn == null && uowTransaction == null)
            {
                id = (await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.Accounts"" (""UserId"", ""Name"", ""IsMain"", ""Currency"", ""StockPrice"", ""CreatedDate"", ""ModifiedDate"")
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
            await Dapper.ExecuteAsync(@"UPDATE ""Accountant.Accounts"" SET ""Name"" = @Name, ""Currency"" = @Currency, ""StockPrice"" = @StockPrice, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id", account);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var transaction = Dapper.BeginTransaction();

            var deletedEntryExists = await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.DeletedEntities""
                                                            WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                                            new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id });

            if (deletedEntryExists)
            {
                await Dapper.QueryAsync<int>(@"UPDATE ""Accountant.DeletedEntities"" SET ""DeletedDate"" = @DeletedDate
                                             WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""EntityId"" = @EntityId",
                                             new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id, DeletedDate = DateTime.UtcNow },
                                             transaction);
            }
            else
            {
                await Dapper.QueryAsync<int>(@"INSERT INTO ""Accountant.DeletedEntities"" (""UserId"", ""EntityType"", ""EntityId"", ""DeletedDate"")
                                         VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                         new { UserId = userId, EntityType = (short)EntityType.Account, EntityId = id, DeletedDate = DateTime.UtcNow },
                                         transaction);
            }

            await Dapper.ExecuteAsync(@"DELETE FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId }, transaction);

            transaction.Commit();
        }
    }
}
