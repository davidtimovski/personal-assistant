using System.Reflection;
using Core.Application.Contracts;
using Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application;

public static class IoC
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IPushSubscriptionService, PushSubscriptionService>();
        services.AddTransient<ITooltipService, TooltipService>();
        services.AddTransient<ICsvService, CsvService>();
        services.AddTransient<IFriendshipService, FriendshipService>();

        return services;
    }
}
