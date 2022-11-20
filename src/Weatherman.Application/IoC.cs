using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Weatherman.Application;

public static class IoC
{
    public static IServiceCollection AddWeatherman(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
