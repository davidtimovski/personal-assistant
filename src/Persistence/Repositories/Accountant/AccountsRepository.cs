using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.Accountant.Accounts;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant;

public class AccountsRepository : BaseRepository, IAccountsRepository
{
    public AccountsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Account> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Account>(@"SELECT * FROM ""Accountant.Accounts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Account, DeletedDate = fromDate });
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool IsMain(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""Accountant.Accounts"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId AND ""IsMain""",
            new { Id = id, UserId = userId });
    }

    public async Task<int> CreateAsync(Account account, IDbConnection uowConn = null, IDbTransaction uowTransaction = null)
    {
        int id;
        const string query = @"INSERT INTO ""Accountant.Accounts"" (""UserId"", ""Name"", ""IsMain"", ""Currency"", ""StockPrice"", ""CreatedDate"", ""ModifiedDate"")
                                   VALUES (@UserId, @Name, @IsMain, @Currency, @StockPrice, @CreatedDate, @ModifiedDate) returning ""Id""";

        if (uowConn == null && uowTransaction == null)
        {
            using IDbConnection conn = OpenConnection();

            id = (await conn.QueryAsync<int>(query, account)).Single();
        }
        else
        {
            id = (await uowConn.QueryAsync<int>(query, account, uowTransaction)).Single();
        }

        return id;
    }

    public async Task UpdateAsync(Account account)
    {
        Account dbAccount = EFContext.Accounts.Find(account.Id);

        dbAccount.Name = account.Name;
        dbAccount.Currency = account.Currency;
        dbAccount.StockPrice = account.StockPrice;
        dbAccount.ModifiedDate = account.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var deletedEntryExists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
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