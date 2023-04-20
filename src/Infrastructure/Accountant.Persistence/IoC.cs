using Accountant.Application.Contracts.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.Accountant;

namespace Accountant.Persistence;

public static class IoC
{
    public static IServiceCollection AddAccountantPersistence(this IServiceCollection services)
    {
        services.AddTransient<ITransactionsRepository, TransactionsRepository>();

        return services;
    }
}
