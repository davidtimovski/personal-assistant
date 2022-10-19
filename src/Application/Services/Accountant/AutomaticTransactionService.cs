using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.AutomaticTransactions;
using Application.Contracts.Accountant.AutomaticTransactions.Models;
using Application.Contracts.Accountant.Common.Models;
using AutoMapper;
using Domain.Entities.Accountant;
using Microsoft.Extensions.Logging;

namespace Application.Services.Accountant;

public class AutomaticTransactionService : IAutomaticTransactionService
{
    private readonly IAutomaticTransactionsRepository _AutomaticTransactionsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AutomaticTransactionService> _logger;

    public AutomaticTransactionService(
        IAutomaticTransactionsRepository AutomaticTransactionsRepository,
        IMapper mapper,
        ILogger<AutomaticTransactionService> logger)
    {
        _AutomaticTransactionsRepository = AutomaticTransactionsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<AutomaticTransactionDto> GetAll(GetAll model)
    {
        try
        {
            var automaticTransaction = _AutomaticTransactionsRepository.GetAll(model.UserId, model.FromModifiedDate);

            var automaticTransactionDtos = automaticTransaction.Select(x => _mapper.Map<AutomaticTransactionDto>(x));

            return automaticTransactionDtos;
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
            return _AutomaticTransactionsRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public Task<int> CreateAsync(CreateAutomaticTransaction model)
    {
        try
        {
            var automaticTransaction = _mapper.Map<AutomaticTransaction>(model);

            if (automaticTransaction.Description != null)
            {
                automaticTransaction.Description = automaticTransaction.Description.Trim();
            }

            return _AutomaticTransactionsRepository.CreateAsync(automaticTransaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateAutomaticTransaction model)
    {
        try
        {
            var automaticTransaction = _mapper.Map<AutomaticTransaction>(model);

            if (automaticTransaction.Description != null)
            {
                automaticTransaction.Description = automaticTransaction.Description.Trim();
            }

            await _AutomaticTransactionsRepository.UpdateAsync(automaticTransaction);
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
            await _AutomaticTransactionsRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }
}
