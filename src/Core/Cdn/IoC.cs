using Cdn.Configuration;
using CloudinaryDotNet;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cdn;

public static class IoC
{
    /// <summary>
    /// Registers the <paramref cref="ICdnService"/> interface with Cloudinary.
    /// </summary>
    public static IServiceCollection AddCdn(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        services.AddOptions<CloudinaryConfig>()
            .Bind(configuration.GetSection("Cloudinary"));

        services.AddSingleton<ICdnService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<CloudinaryConfig>>().Value;
            var logger = sp.GetRequiredService<ILogger<CloudinaryService>>();

            return new CloudinaryService(
                cloudinaryAccount: new Account(config.CloudName, config.ApiKey, config.ApiSecret),
                environmentName,
                new Uri(config.DefaultImageUris.Profile),
                new Uri(config.DefaultImageUris.Recipe),
                new HttpClient(),
                logger);
        });

        services.AddTransient<IValidator<UploadTempImage>, UploadTempImageValidator>();

        return services;
    }
}
