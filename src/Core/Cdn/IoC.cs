using Cdn.Configuration;
using CloudinaryDotNet;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Cdn;

public static class IoC
{
    /// <summary>
    /// Registers the <paramref cref="ICdnService"/> interface with Cloudinary.
    /// </summary>
    public static IServiceCollection AddCdn(
        this IServiceCollection services,
        CloudinaryConfig config,
        string environmentName)
    {
        services.AddSingleton<ICdnService>(new CloudinaryService(
            cloudinaryAccount: new Account(config.CloudName, config.ApiKey, config.ApiSecret),
            environmentName,
            config.DefaultImageUris.Profile,
            config.DefaultImageUris.Recipe,
            new HttpClient()));

        services.AddTransient<IValidator<UploadTempImage>, UploadTempImageValidator>();

        return services;
    }
}
