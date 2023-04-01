using System.Data;
using Accountant.Application.Contracts.Accounts;
using Application.Domain.Accountant;
using Core.Persistence;
using Dapper;

namespace Persistence.Repositories.Accountant;

public class AccountsRepository : BaseRepository, IAccountsRepository
{
    public AccountsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM accountant.accounts WHERE id = @Id AND user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool HasMain(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM accountant.accounts WHERE user_id = @UserId AND is_main", new { UserId = userId });
    }

    public bool IsMain(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM accountant.accounts WHERE id = @Id AND user_id = @UserId AND is_main",
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
        Account dbAccount = EFContext.Accounts.First(x => x.Id == account.Id && x.UserId == account.UserId);

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
