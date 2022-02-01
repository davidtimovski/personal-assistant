using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Categories.Models;
using Application.Contracts.Accountant.Common.Models;

namespace Application.Contracts.Accountant.Categories
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDto> GetAll(GetAll model);
        IEnumerable<int> GetDeletedIds(GetDeletedIds model);
        Task<int> CreateAsync(CreateCategory model);
        Task UpdateAsync(UpdateCategory model);
        Task DeleteAsync(int id, int userId);
    }
}
