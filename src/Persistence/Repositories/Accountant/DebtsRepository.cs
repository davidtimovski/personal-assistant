using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Debts;
using Dapper;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant;

public class DebtsRepository : BaseRepository, IDebtsRepository
{
    public DebtsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Debt> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Debt>(@"SELECT * FROM ""Accountant.Debts"" WHERE ""UserId"" = @UserId AND ""ModifiedDate"" > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT ""EntityId"" FROM ""Accountant.DeletedEntities"" WHERE ""UserId"" = @UserId AND ""EntityType"" = @EntityType AND ""DeletedDate"" > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Debt, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(Debt debt)
    {
        EFContext.Debts.Add(debt);
        await EFContext.SaveChangesAsync();
        return debt.Id;
    }

    public async Task UpdateAsync(Debt debt)
    {
        Debt dbDebt = EFContext.Debts.Find(debt.Id);

        dbDebt.Person = debt.Person;
        dbDebt.Amount = debt.Amount;
        dbDebt.Currency = debt.Currency;
        dbDebt.Description = debt.Description;
        dbDebt.UserIsDebtor = debt.UserIsDebtor;
        dbDebt.ModifiedDate = debt.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.Debt && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.Debt,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var debt = EFContext.Debts.First(x => x.Id == id && x.UserId == userId);
        EFContext.Debts.Remove(debt);

        await EFContext.SaveChangesAsync();
    }
}
