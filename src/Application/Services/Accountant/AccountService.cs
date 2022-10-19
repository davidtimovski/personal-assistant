using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.Common.Models;
using AutoMapper;
using Domain.Entities.Accountant;
using Microsoft.Extensions.Logging;

namespace Application.Services.Accountant;

public class AccountService : IAccountService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountsRepository accountsRepository,
        IMapper mapper,
        ILogger<AccountService> logger)
    {
        _accountsRepository = accountsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<AccountDto> GetAll(GetAll model)
    {
        try
        {
            var accounts = _accountsRepository.GetAll(model.UserId, model.FromModifiedDate);

            var accountDtos = accounts.Select(x => _mapper.Map<AccountDto>(x));

            return accountDtos;
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
            return _accountsRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public Task<int> CreateAsync(CreateAccount model)
    {
        try
        {
            var account = _mapper.Map<Account>(model);

            account.Name = account.Name.Trim();

            return _accountsRepository.CreateAsync(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task CreateMainAsync(CreateMainAccount model)
    {
        try
        {
            if (_accountsRepository.HasMain(model.UserId))
            {
                throw new ArgumentException("User already has a main account.");
            }

            var now = DateTime.UtcNow;

            var account = _mapper.Map<Account>(model);
            account.IsMain = true;
            account.Currency = "EUR";
            account.CreatedDate = now;
            account.ModifiedDate = now;

            await _accountsRepository.CreateAsync(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateMainAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateAccount model)
    {
        try
        {
            var account = _mapper.Map<Account>(model);

            account.Name = account.Name.Trim();

            await _accountsRepository.UpdateAsync(account);
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
            if (_accountsRepository.IsMain(id, userId))
            {
                throw new ArgumentException("Cannot delete main account.");
            }

            await _accountsRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }
}
