using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using CloudinaryDotNet;
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
        string cloudName,
        string apiKey,
        string apiSecret,
        string environmentName,
        string defaultProfileUri,
        string defaultRecipeUri)
    {
        services.AddSingleton<ICdnService>(new CloudinaryService(
            cloudinaryAccount: new Account(cloudName, apiKey, apiSecret),
            environmentName,
            defaultProfileUri,
            defaultRecipeUri,
            new HttpClient()));

        services.AddTransient<IValidator<UploadTempImage>, UploadTempImageValidator>();

        return services;
    }
}
