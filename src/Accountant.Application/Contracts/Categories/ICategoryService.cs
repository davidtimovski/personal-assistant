using Accountant.Application.Contracts.Categories.Models;
using Accountant.Application.Contracts.Common.Models;

namespace Accountant.Application.Contracts.Categories;

public interface ICategoryService
{
    IEnumerable<CategoryDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateCategory model);
    Task UpdateAsync(UpdateCategory model);
    Task DeleteAsync(int id, int userId);
}
