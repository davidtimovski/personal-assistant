using Accountant.Application.Contracts.AutomaticTransactions;
using Accountant.Application.Contracts.Categories;
using Accountant.Application.Contracts.Debts;
using Accountant.Application.Contracts.Sync;
using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Contracts.UpcomingExpenses;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.Accountant;

namespace Accountant.Persistence;

public static class IoC
{
    public static IServiceCollection AddAccountantPersistence(this IServiceCollection services)
    {
        services.AddTransient<ICategoriesRepository, CategoriesRepository>();
        services.AddTransient<ITransactionsRepository, TransactionsRepository>();
        services.AddTransient<IUpcomingExpensesRepository, UpcomingExpensesRepository>();
        services.AddTransient<IDebtsRepository, DebtsRepository>();
        services.AddTransient<IAutomaticTransactionsRepository, AutomaticTransactionsRepository>();
        services.AddTransient<ISyncRepository, SyncRepository>();

        return services;
    }
}
