using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
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

        public async Task<IEnumerable<AccountDto>> GetAllAsync(GetAll model)
        {
            var accounts = await _accountsRepository.GetAllAsync(model.UserId, model.FromModifiedDate);

            var accountDtos = accounts.Select(x => _mapper.Map<AccountDto>(x));

            return accountDtos;
        }

        public Task<IEnumerable<int>> GetDeletedIdsAsync(GetDeletedIds model)
        {
            return _accountsRepository.GetDeletedIdsAsync(model.UserId, model.FromDate);
        }

        public Task<int> CreateAsync(CreateAccount model)
        {
            var account = _mapper.Map<Account>(model);
            return _accountsRepository.CreateAsync(account);
        }

        public async Task CreateMainAsync(CreateMainAccount model)
        {
            var account = _mapper.Map<Account>(model);
            await _accountsRepository.CreateAsync(account);
        }

        public async Task UpdateAsync(UpdateAccount model)
        {
            var account = _mapper.Map<Account>(model);
            await _accountsRepository.UpdateAsync(account);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            if (await _accountsRepository.IsMainAsync(id, userId))
            {
                throw new ArgumentException("Cannot delete main account.");
            }

            await _accountsRepository.DeleteAsync(id, userId);
        }
    }
}
