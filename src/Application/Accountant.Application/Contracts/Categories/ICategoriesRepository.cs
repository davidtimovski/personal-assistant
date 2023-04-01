using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Categories;

public interface ICategoriesRepository
{
    Task<int> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id, int userId);
}
