using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Categories;

public interface ICategoriesRepository
{
    IEnumerable<Category> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id, int userId);
}
