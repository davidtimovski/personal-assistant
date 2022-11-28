using System.Reflection;
using Accountant.Application.Contracts.Sync;
using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Accountant.Application;

public static class IoC
{
    public static IServiceCollection AddAccountant(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<ITransactionService, TransactionService>();
        services.AddTransient<ISyncService, SyncService>();

        return services;
    }
}
