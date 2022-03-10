using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Accounts;
using Dapper;
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

    public async Task<int> CreateAsync(Account account)
    {
        EFContext.Accounts.Add(account);
        await EFContext.SaveChangesAsync();
        return account.Id;
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
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.Account && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.Account,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var account = EFContext.Accounts.First(x => x.Id == id && x.UserId == userId);
        EFContext.Accounts.Remove(account);

        await EFContext.SaveChangesAsync();
    }
}
