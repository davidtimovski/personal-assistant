using System.Data;
using Accountant.Application.Contracts.AutomaticTransactions;
using Application.Domain.Accountant;
using Core.Persistence;
using Dapper;

namespace Persistence.Repositories.Accountant;

public class AutomaticTransactionsRepository : BaseRepository, IAutomaticTransactionsRepository
{
    public AutomaticTransactionsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public async Task<int> CreateAsync(AutomaticTransaction automaticTransaction)
    {
        EFContext.AutomaticTransactions.Add(automaticTransaction);
        await EFContext.SaveChangesAsync();
        return automaticTransaction.Id;
    }

    public async Task UpdateAsync(AutomaticTransaction automaticTransaction)
    {
        AutomaticTransaction dbAutomaticTransaction = EFContext.AutomaticTransactions.First(x => x.Id == automaticTransaction.Id && x.UserId == automaticTransaction.UserId);

        dbAutomaticTransaction.IsDeposit = automaticTransaction.IsDeposit;
        dbAutomaticTransaction.CategoryId = automaticTransaction.CategoryId;
        dbAutomaticTransaction.Amount = automaticTransaction.Amount;
        dbAutomaticTransaction.Currency = automaticTransaction.Currency;
        dbAutomaticTransaction.Description = automaticTransaction.Description;
        dbAutomaticTransaction.DayInMonth = automaticTransaction.DayInMonth;
        dbAutomaticTransaction.ModifiedDate = automaticTransaction.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.AutomaticTransaction && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.AutomaticTransaction,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var AutomaticTransaction = EFContext.AutomaticTransactions.First(x => x.Id == id && x.UserId == userId);
        EFContext.AutomaticTransactions.Remove(AutomaticTransaction);

        await EFContext.SaveChangesAsync();
    }
}
