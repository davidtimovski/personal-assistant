using System.Data;
using Accountant.Application.Contracts.Debts;
using Dapper;
using Application.Domain.Accountant;

namespace Persistence.Repositories.Accountant;

public class DebtsRepository : BaseRepository, IDebtsRepository
{
    public DebtsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Debt> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Debt>(@"SELECT * FROM accountant.debts WHERE user_id = @UserId AND modified_date > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT entity_id FROM accountant.deleted_entities WHERE user_id = @UserId AND entity_type = @EntityType AND deleted_date > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Debt, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(Debt debt)
    {
        EFContext.Debts.Add(debt);
        await EFContext.SaveChangesAsync();
        return debt.Id;
    }

    public async Task<int> CreateMergedAsync(Debt debt)
    {
        var otherDebtWithPerson = EFContext.Debts.Where(x => x.UserId == debt.UserId && x.Person.ToLower() == debt.Person.ToLower()).ToList();
        foreach (var otherDebt in otherDebtWithPerson)
        {
            EFContext.Debts.Remove(otherDebt);

            var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == debt.UserId && x.EntityType == EntityType.Debt && x.EntityId == otherDebt.Id);
            if (deletedEntity == null)
            {
                EFContext.DeletedEntities.Add(new DeletedEntity
                {
                    UserId = debt.UserId,
                    EntityType = EntityType.Debt,
                    EntityId = otherDebt.Id,
                    DeletedDate = debt.CreatedDate
                });
            }
            else
            {
                deletedEntity.DeletedDate = debt.CreatedDate;
            }
        }

        EFContext.Debts.Add(debt);
        await EFContext.SaveChangesAsync();
        return debt.Id;
    }

    public async Task UpdateAsync(Debt debt)
    {
        Debt dbDebt = EFContext.Debts.First(x => x.Id == debt.Id && x.UserId == debt.UserId);

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
