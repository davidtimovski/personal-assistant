﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Sync;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant;

public class SyncRepository : BaseRepository, ISyncRepository
{
    public SyncRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public async Task SyncAsync(IEnumerable<Account> accounts, IEnumerable<Category> categories, IEnumerable<Transaction> transactions, IEnumerable<UpcomingExpense> upcomingExpenses, IEnumerable<Debt> debts)
    {
        EFContext.Accounts.AddRange(accounts);
        EFContext.Categories.AddRange(categories);
        EFContext.Transactions.AddRange(transactions);
        EFContext.UpcomingExpenses.AddRange(upcomingExpenses);
        EFContext.Debts.AddRange(debts);

        await EFContext.SaveChangesAsync();
    }
}