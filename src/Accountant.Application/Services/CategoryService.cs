using Accountant.Application.Contracts.Categories;
using Accountant.Application.Contracts.Categories.Models;
using Accountant.Application.Contracts.Common.Models;
using AutoMapper;
using Domain.Accountant;
using Microsoft.Extensions.Logging;

namespace Accountant.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoriesRepository categoriesRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoriesRepository = categoriesRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<CategoryDto> GetAll(GetAll model)
    {
        try
        {
            var categories = _categoriesRepository.GetAll(model.UserId, model.FromModifiedDate);

            var categoryDtos = categories.Select(x => _mapper.Map<CategoryDto>(x));

            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
    }

    public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
    {
        try
        {
            return _categoriesRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public Task<int> CreateAsync(CreateCategory model)
    {
        try
        {
            if (model.Type == CategoryType.DepositOnly)
            {
                model.GenerateUpcomingExpense = false;
            }

            var category = _mapper.Map<Category>(model);

            category.Name = category.Name.Trim();

            return _categoriesRepository.CreateAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateCategory model)
    {
        try
        {
            if (model.Type == CategoryType.DepositOnly)
            {
                model.GenerateUpcomingExpense = false;
            }

            var category = _mapper.Map<Category>(model);

            category.Name = category.Name.Trim();

            await _categoriesRepository.UpdateAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public async Task DeleteAsync(int id, int userId)
    {
        try
        {
            await _categoriesRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }
}
