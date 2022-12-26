using System.Data;
using Accountant.Application.Contracts.UpcomingExpenses;
using Dapper;
using Application.Domain.Accountant;

namespace Persistence.Repositories.Accountant;

public class UpcomingExpensesRepository : BaseRepository, IUpcomingExpensesRepository
{
    public UpcomingExpensesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<UpcomingExpense> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<UpcomingExpense>(@"SELECT * FROM accountant.upcoming_expenses WHERE user_id = @UserId AND modified_date > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT entity_id FROM accountant.deleted_entities WHERE user_id = @UserId AND entity_type = @EntityType AND deleted_date > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(UpcomingExpense upcomingExpense)
    {
        upcomingExpense.Date = upcomingExpense.Date.ToUniversalTime();

        EFContext.UpcomingExpenses.Add(upcomingExpense);
        await EFContext.SaveChangesAsync();
        return upcomingExpense.Id;
    }

    public async Task UpdateAsync(UpcomingExpense upcomingExpense)
    {
        UpcomingExpense dbUpcomingExpense = EFContext.UpcomingExpenses.First(x => x.Id == upcomingExpense.Id && x.UserId == upcomingExpense.UserId);

        dbUpcomingExpense.CategoryId = upcomingExpense.CategoryId;
        dbUpcomingExpense.Amount = upcomingExpense.Amount;
        dbUpcomingExpense.Currency = upcomingExpense.Currency;
        dbUpcomingExpense.Description = upcomingExpense.Description;
        dbUpcomingExpense.Date = upcomingExpense.Date.ToUniversalTime();
        dbUpcomingExpense.ModifiedDate = upcomingExpense.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.UpcomingExpense && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.UpcomingExpense,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var upcomingExpense = EFContext.UpcomingExpenses.First(x => x.Id == id && x.UserId == userId);
        EFContext.UpcomingExpenses.Remove(upcomingExpense);

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteOldAsync(int userId, DateTime before)
    {
        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var toDelete = await conn.QueryAsync<UpcomingExpense>(@"SELECT * FROM accountant.upcoming_expenses WHERE user_id = @UserId AND date < @Date",
            new { UserId = userId, Date = before });

        foreach (var upcomingExpense in toDelete)
        {
            var deletedEntryExists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                           FROM accountant.deleted_entities
                                                                           WHERE user_id = @UserId AND entity_type = @EntityType AND entity_id = @EntityId",
                new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id });

            if (deletedEntryExists)
            {
                await conn.QueryAsync<int>(@"UPDATE accountant.deleted_entities SET deleted_date = @DeletedDate
                                             WHERE user_id = @UserId AND entity_type = @EntityType AND entity_id = @EntityId",
                    new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                    transaction);
            }
            else
            {
                await conn.QueryAsync<int>(@"INSERT INTO accountant.deleted_entities (user_id, entity_type, entity_id, deleted_date)
                                             VALUES (@UserId, @EntityType, @EntityId, @DeletedDate)",
                    new { UserId = userId, EntityType = (short)EntityType.UpcomingExpense, EntityId = upcomingExpense.Id, DeletedDate = DateTime.UtcNow },
                    transaction);
            }

            await conn.ExecuteAsync(@"DELETE FROM accountant.upcoming_expenses WHERE id = @Id AND user_id = @UserId",
                new { upcomingExpense.Id, UserId = userId }, transaction);
        }

        transaction.Commit();
    }
}
