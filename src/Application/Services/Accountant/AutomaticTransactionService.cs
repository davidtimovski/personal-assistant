using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.AutomaticTransactions;
using Application.Contracts.Accountant.AutomaticTransactions.Models;
using Application.Contracts.Accountant.Common.Models;
using AutoMapper;
using Domain.Entities.Accountant;

namespace Application.Services.Accountant;

public class AutomaticTransactionService : IAutomaticTransactionService
{
    private readonly IAutomaticTransactionsRepository _AutomaticTransactionsRepository;
    private readonly IMapper _mapper;

    public AutomaticTransactionService(
        IAutomaticTransactionsRepository AutomaticTransactionsRepository,
        IMapper mapper)
    {
        _AutomaticTransactionsRepository = AutomaticTransactionsRepository;
        _mapper = mapper;
    }

    public IEnumerable<AutomaticTransactionDto> GetAll(GetAll model)
    {
        var automaticTransaction = _AutomaticTransactionsRepository.GetAll(model.UserId, model.FromModifiedDate);

        var automaticTransactionDtos = automaticTransaction.Select(x => _mapper.Map<AutomaticTransactionDto>(x));

        return automaticTransactionDtos;
    }

    public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
    {
        return _AutomaticTransactionsRepository.GetDeletedIds(model.UserId, model.FromDate);
    }

    public Task<int> CreateAsync(CreateAutomaticTransaction model)
    {
        var automaticTransaction = _mapper.Map<AutomaticTransaction>(model);

        if (automaticTransaction.Description != null)
        {
            automaticTransaction.Description = automaticTransaction.Description.Trim();
        }

        return _AutomaticTransactionsRepository.CreateAsync(automaticTransaction);
    }

    public async Task UpdateAsync(UpdateAutomaticTransaction model)
    {
        var automaticTransaction = _mapper.Map<AutomaticTransaction>(model);

        if (automaticTransaction.Description != null)
        {
            automaticTransaction.Description = automaticTransaction.Description.Trim();
        }

        await _AutomaticTransactionsRepository.UpdateAsync(automaticTransaction);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        await _AutomaticTransactionsRepository.DeleteAsync(id, userId);
    }
}
