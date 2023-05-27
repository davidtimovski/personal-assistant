using Microsoft.Extensions.DependencyInjection;

namespace Weatherman.Application;

public static class IoC
{
    public static IServiceCollection AddWeatherman(this IServiceCollection services)
    {
        return services;
    }
}
