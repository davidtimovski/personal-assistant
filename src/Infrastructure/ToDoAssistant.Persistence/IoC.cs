using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Persistence.Repositories;

namespace ToDoAssistant.Persistence;

public static class IoC
{
    public static IServiceCollection AddToDoAssistantPersistence(this IServiceCollection services, string connectionString)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<ToDoAssistantContext>(opt =>
        {
            opt.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IListsRepository, ListsRepository>();
        services.AddTransient<ITasksRepository, TasksRepository>();
        services.AddTransient<INotificationsRepository, NotificationsRepository>();

        return services;
    }
}
