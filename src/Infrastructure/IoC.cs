using System.Net.Http;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Contracts.Common;
using Infrastructure.Cdn;
using Infrastructure.Currency;
using Infrastructure.Sender;

namespace Infrastructure
{
    public static class IoC
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            string environmentName)
        {
            services.AddTransient<ICurrencyService, CurrencyService>();

            services.AddSingleton<ISenderService, SenderService>();

            services.AddSingleton<ICdnService>(new CloudinaryService(
                cloudinaryAccount: new Account(
                    configuration["Cloudinary:CloudName"],
                    configuration["Cloudinary:ApiKey"],
                    configuration["Cloudinary:ApiSecret"]),
                environmentName,
                configuration["DefaultImageUris:Profile"],
                configuration["DefaultImageUris:Recipe"],
                new HttpClient()));

            return services;
        }
    }
}