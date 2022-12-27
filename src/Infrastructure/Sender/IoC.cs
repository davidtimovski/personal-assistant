using Core.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Sender;

public static class IoC
{
    /// <summary>
    /// Registers the <paramref cref="ISenderService"/> interface used for enqueuing emails and push notifications.
    /// </summary>
    public static IServiceCollection AddSender(this IServiceCollection services)
    {
        services.AddSingleton<ISenderService, SenderService>();

        return services;
    }
}
