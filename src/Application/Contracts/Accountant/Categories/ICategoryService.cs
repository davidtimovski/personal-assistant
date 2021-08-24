using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Categories.Models;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Categories
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
