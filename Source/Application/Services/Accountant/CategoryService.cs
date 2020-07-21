using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Categories.Models;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IMapper _mapper;

        public CategoryService(
            ICategoriesRepository categoriesRepository,
            IMapper mapper)
        {
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(GetAll model)
        {
            var categories = await _categoriesRepository.GetAllAsync(model.UserId, model.FromModifiedDate);

            var categoryDtos = categories.Select(x => _mapper.Map<CategoryDto>(x));

            return categoryDtos;
        }

        public Task<IEnumerable<int>> GetDeletedIdsAsync(GetDeletedIds model)
        {
            return _categoriesRepository.GetDeletedIdsAsync(model.UserId, model.FromDate);
        }

        public Task<int> CreateAsync(CreateCategory model)
        {
            if (model.Type == CategoryType.DepositOnly)
            {
                model.GenerateUpcomingExpense = false;
            }

            var category = _mapper.Map<Category>(model);

            return _categoriesRepository.CreateAsync(category);
        }

        public async Task UpdateAsync(UpdateCategory model)
        {
            if (model.Type == CategoryType.DepositOnly)
            {
                model.GenerateUpcomingExpense = false;
            }

            var category = _mapper.Map<Category>(model);

            await _categoriesRepository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await _categoriesRepository.DeleteAsync(id, userId);
        }
    }
}
