using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Persistence.Models;
using ToDoAssistant.Persistence.Repositories;

namespace ToDoAssistant.Persistence;

public static class IoC
{
    public static IServiceCollection AddToDoAssistantPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.Get<PersistenceConfiguration>();
        if (config is null)
        {
            throw new ArgumentNullException("Persistence configuration is missing");
        }

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<ToDoAssistantContext>(opt =>
        {
            opt.UseNpgsql(config.ConnectionString)
               .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IListsRepository, ListsRepository>();
        services.AddTransient<ITasksRepository, TasksRepository>();
        services.AddTransient<INotificationsRepository, NotificationsRepository>();

        return services;
    }
}
