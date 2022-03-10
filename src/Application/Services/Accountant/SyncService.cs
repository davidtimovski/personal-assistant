using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Sync;
using Application.Contracts.Accountant.Sync.Models;
using AutoMapper;
using Domain.Entities.Accountant;

namespace Application.Services.Accountant;

public class SyncService : ISyncService
{
    private readonly ISyncRepository _syncRepository;
    private readonly IMapper _mapper;

    public SyncService(
        ISyncRepository syncRepository,
        IMapper mapper)
    {
        _syncRepository = syncRepository;
        _mapper = mapper;
    }

    public async Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model)
    {
        var accounts = model.Accounts.Select(x => _mapper.Map<Account>(x)).ToList();
        var categories = model.Categories.Select(x => _mapper.Map<Category>(x)).ToList();
        var transactions = model.Transactions.Select(x => _mapper.Map<Transaction>(x)).ToList();
        var upcomingExpenses = model.UpcomingExpenses.Select(x => _mapper.Map<UpcomingExpense>(x)).ToList();
        var debts = model.Debts.Select(x => _mapper.Map<Debt>(x)).ToList();

        await _syncRepository.SyncAsync(accounts, categories, transactions, upcomingExpenses, debts);

        var accountIds = accounts.Select(x => x.Id).ToArray();
        var categoryIds = categories.Select(x => x.Id).ToArray();
        var transactionIds = transactions.Select(x => x.Id).ToArray();
        var upcomingExpenseIds = upcomingExpenses.Select(x => x.Id).ToArray();
        var debtIds = debts.Select(x => x.Id).ToArray();

        return new SyncedEntityIds(accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds);
    }
}
