using Accountant.Application.Contracts.Common.Models;
using Accountant.Application.Contracts.Debts;
using Accountant.Application.Contracts.Debts.Models;
using AutoMapper;
using Domain.Accountant;
using Microsoft.Extensions.Logging;

namespace Accountant.Application.Services;

public class DebtService : IDebtService
{
    private readonly IDebtsRepository _debtsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DebtService> _logger;

    public DebtService(
        IDebtsRepository debtsRepository,
        IMapper mapper,
        ILogger<DebtService> logger)
    {
        _debtsRepository = debtsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<DebtDto> GetAll(GetAll model)
    {
        try
        {
            var debt = _debtsRepository.GetAll(model.UserId, model.FromModifiedDate);

            var debtDtos = debt.Select(x => _mapper.Map<DebtDto>(x));

            return debtDtos;
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
            return _debtsRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public Task<int> CreateAsync(CreateDebt model)
    {
        try
        {
            var debt = _mapper.Map<Debt>(model);

            debt.Person = debt.Person.Trim();
            if (debt.Description != null)
            {
                debt.Description = debt.Description.Trim();
            }

            return _debtsRepository.CreateAsync(debt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public Task<int> CreateMergedAsync(CreateDebt model)
    {
        try
        {
            var debt = _mapper.Map<Debt>(model);

            debt.Person = debt.Person.Trim();
            if (debt.Description != null)
            {
                debt.Description = debt.Description.Trim();
            }

            return _debtsRepository.CreateMergedAsync(debt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateMergedAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateDebt model)
    {
        try
        {
            var debt = _mapper.Map<Debt>(model);

            debt.Person = debt.Person.Trim();
            if (debt.Description != null)
            {
                debt.Description = debt.Description.Trim();
            }

            await _debtsRepository.UpdateAsync(debt);
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
            await _debtsRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }
}
