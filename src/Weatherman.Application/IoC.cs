using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Weatherman.Application;

public static class IoC
{
    public static IServiceCollection AddWeatherman(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
