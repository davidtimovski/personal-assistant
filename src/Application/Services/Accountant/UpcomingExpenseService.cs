using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Common.Models;
using Application.Contracts.Accountant.UpcomingExpenses;
using Application.Contracts.Accountant.UpcomingExpenses.Models;
using AutoMapper;
using Domain.Entities.Accountant;
using Microsoft.Extensions.Logging;

namespace Application.Services.Accountant;

public class UpcomingExpenseService : IUpcomingExpenseService
{
    private readonly IUpcomingExpensesRepository _upcomingExpensesRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpcomingExpenseService> _logger;

    public UpcomingExpenseService(
        IUpcomingExpensesRepository upcomingExpensesRepository,
        IMapper mapper,
        ILogger<UpcomingExpenseService> logger)
    {
        _upcomingExpensesRepository = upcomingExpensesRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<UpcomingExpenseDto> GetAll(GetAll model)
    {
        try
        {
            var upcomingExpenses = _upcomingExpensesRepository.GetAll(model.UserId, model.FromModifiedDate);

            var upcomingExpenseDtos = upcomingExpenses.Select(x => _mapper.Map<UpcomingExpenseDto>(x));

            return upcomingExpenseDtos;
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
            return _upcomingExpensesRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public Task<int> CreateAsync(CreateUpcomingExpense model)
    {
        try
        {
            var upcomingExpense = _mapper.Map<UpcomingExpense>(model);

            if (upcomingExpense.Description != null)
            {
                upcomingExpense.Description = upcomingExpense.Description.Trim();
            }

            return _upcomingExpensesRepository.CreateAsync(upcomingExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateUpcomingExpense model)
    {
        try
        {
            var upcomingExpense = _mapper.Map<UpcomingExpense>(model);

            if (upcomingExpense.Description != null)
            {
                upcomingExpense.Description = upcomingExpense.Description.Trim();
            }

            await _upcomingExpensesRepository.UpdateAsync(upcomingExpense);
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
            await _upcomingExpensesRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }

    public async Task DeleteOldAsync(int userId)
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0);
            await _upcomingExpensesRepository.DeleteOldAsync(userId, startOfMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteOldAsync)}");
            throw;
        }
    }
}
