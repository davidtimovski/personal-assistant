using Accountant.Application.Contracts.Sync;
using Accountant.Application.Contracts.Sync.Models;
using AutoMapper;
using Domain.Accountant;
using Microsoft.Extensions.Logging;

namespace Accountant.Application.Services;

public class SyncService : ISyncService
{
    private readonly ISyncRepository _syncRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        ISyncRepository syncRepository,
        IMapper mapper,
        ILogger<SyncService> logger)
    {
        _syncRepository = syncRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model)
    {
        try
        {
            var accounts = model.Accounts.Select(x => _mapper.Map<Account>(x)).ToList();
            var categories = model.Categories.Select(x => _mapper.Map<Category>(x)).ToList();
            var transactions = model.Transactions.Select(x => _mapper.Map<Transaction>(x)).ToList();
            var upcomingExpenses = model.UpcomingExpenses.Select(x => _mapper.Map<UpcomingExpense>(x)).ToList();
            var debts = model.Debts.Select(x => _mapper.Map<Debt>(x)).ToList();
            var automaticTransactions = model.AutomaticTransactions.Select(x => _mapper.Map<AutomaticTransaction>(x)).ToList();

            await _syncRepository.SyncAsync(accounts, categories, transactions, upcomingExpenses, debts, automaticTransactions);

            var accountIds = accounts.Select(x => x.Id).ToArray();
            var categoryIds = categories.Select(x => x.Id).ToArray();
            var transactionIds = transactions.Select(x => x.Id).ToArray();
            var upcomingExpenseIds = upcomingExpenses.Select(x => x.Id).ToArray();
            var debtIds = debts.Select(x => x.Id).ToArray();
            var automaticTransactionIds = automaticTransactions.Select(x => x.Id).ToArray();

            return new SyncedEntityIds(accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SyncEntitiesAsync)}");
            throw;
        }
    }
}
