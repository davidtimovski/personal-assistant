using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Services;

namespace ToDoAssistant.Application;

public static class IoC
{
    public static IServiceCollection AddToDoAssistant(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<IValidator<CreateList>, CreateListValidator>();
        services.AddTransient<IValidator<UpdateList>, UpdateListValidator>();
        services.AddTransient<IValidator<ShareList>, ShareListValidator>();
        services.AddTransient<IValidator<CopyList>, CopyListValidator>();
        services.AddTransient<IValidator<UpdateSharedList>, UpdateSharedListValidator>();
        services.AddTransient<IValidator<CreateTask>, CreateTaskValidator>();
        services.AddTransient<IValidator<BulkCreate>, BulkCreateValidator>();
        services.AddTransient<IValidator<UpdateTask>, UpdateTaskValidator>();

        services.AddTransient<IListService, ListService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<INotificationService, NotificationService>();

        return services;
    }
}
