using System.Reflection;
using Accountant.Application.Contracts.Accounts;
using Accountant.Application.Contracts.AutomaticTransactions;
using Accountant.Application.Contracts.Categories;
using Accountant.Application.Contracts.Debts;
using Accountant.Application.Contracts.Sync;
using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Contracts.UpcomingExpenses;
using Accountant.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accountant.Application;

public static class IoC
{
    public static IServiceCollection AddAccountant(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<ITransactionService, TransactionService>();
        services.AddTransient<IUpcomingExpenseService, UpcomingExpenseService>();
        services.AddTransient<IDebtService, DebtService>();
        services.AddTransient<IAutomaticTransactionService, AutomaticTransactionService>();
        services.AddTransient<ISyncService, SyncService>();

        return services;
    }
}
