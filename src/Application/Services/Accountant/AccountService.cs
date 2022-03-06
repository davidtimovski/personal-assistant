using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.Common.Models;
using Domain.Entities.Accountant;

namespace Application.Services.Accountant;

public class AccountService : IAccountService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IMapper _mapper;

    public AccountService(
        IAccountsRepository accountsRepository,
        IMapper mapper)
    {
        _accountsRepository = accountsRepository;
        _mapper = mapper;
    }

    public IEnumerable<AccountDto> GetAll(GetAll model)
    {
        var accounts = _accountsRepository.GetAll(model.UserId, model.FromModifiedDate);

        var accountDtos = accounts.Select(x => _mapper.Map<AccountDto>(x));

        return accountDtos;
    }

    public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
    {
        return _accountsRepository.GetDeletedIds(model.UserId, model.FromDate);
    }

    public Task<int> CreateAsync(CreateAccount model)
    {
        var account = _mapper.Map<Account>(model);
        return _accountsRepository.CreateAsync(account);
    }

    public async Task CreateMainAsync(CreateMainAccount model)
    {
        var now = DateTime.UtcNow;

        var account = _mapper.Map<Account>(model);
        account.IsMain = true;
        account.Currency = "EUR";
        account.CreatedDate = now;
        account.ModifiedDate = now;

        await _accountsRepository.CreateAsync(account);
    }

    public async Task UpdateAsync(UpdateAccount model)
    {
        var account = _mapper.Map<Account>(model);
        await _accountsRepository.UpdateAsync(account);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        if (_accountsRepository.IsMain(id, userId))
        {
            throw new ArgumentException("Cannot delete main account.");
        }

        await _accountsRepository.DeleteAsync(id, userId);
    }
}
