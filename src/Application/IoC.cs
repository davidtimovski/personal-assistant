using System.Reflection;
using Application.Contracts;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class IoC
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IPushSubscriptionService, PushSubscriptionService>();
        services.AddTransient<ITooltipService, TooltipService>();

        return services;
    }
}
