using System.Data;
using Accountant.Application.Contracts.Categories;
using Dapper;
using Domain.Accountant;

namespace Persistence.Repositories.Accountant;

public class CategoriesRepository : BaseRepository, ICategoriesRepository
{
    public CategoriesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Category> GetAll(int userId, DateTime fromModifiedDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Category>(@"SELECT * FROM accountant_categories WHERE user_id = @UserId AND modified_date > @FromModifiedDate",
            new { UserId = userId, FromModifiedDate = fromModifiedDate });
    }

    public IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT entity_id FROM accountant_deleted_entities WHERE user_id = @UserId AND entity_type = @EntityType AND deleted_date > @DeletedDate",
            new { UserId = userId, EntityType = (short)EntityType.Category, DeletedDate = fromDate });
    }

    public async Task<int> CreateAsync(Category category)
    {
        EFContext.Categories.Add(category);
        await EFContext.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(Category category)
    {
        Category dbCategory = EFContext.Categories.First(x => x.Id == category.Id && x.UserId == category.UserId);

        dbCategory.ParentId = category.ParentId;
        dbCategory.Name = category.Name;
        dbCategory.Type = category.Type;
        dbCategory.GenerateUpcomingExpense = category.GenerateUpcomingExpense;
        dbCategory.IsTax = category.IsTax;
        dbCategory.ModifiedDate = category.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deletedEntity = EFContext.DeletedEntities.FirstOrDefault(x => x.UserId == userId && x.EntityType == EntityType.Category && x.EntityId == id);
        if (deletedEntity == null)
        {
            EFContext.DeletedEntities.Add(new DeletedEntity
            {
                UserId = userId,
                EntityType = EntityType.Category,
                EntityId = id,
                DeletedDate = DateTime.UtcNow
            });
        }
        else
        {
            deletedEntity.DeletedDate = DateTime.UtcNow;
        }

        var category = EFContext.Categories.First(x => x.Id == id && x.UserId == userId);
        EFContext.Categories.Remove(category);

        await EFContext.SaveChangesAsync();
    }
}
