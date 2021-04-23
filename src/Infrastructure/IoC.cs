using System.Net.Http;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Infrastructure.Cdn;
using PersonalAssistant.Infrastructure.Currency;
using PersonalAssistant.Infrastructure.Sender;

namespace PersonalAssistant.Infrastructure
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