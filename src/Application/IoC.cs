using System.Collections.Generic;
using System.Reflection;
using Application.Contracts;
using Application.Contracts.Models;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class IoC
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddTransient<IValidator<UploadTempImage>, UploadTempImageValidator>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IPushSubscriptionService, PushSubscriptionService>();
        services.AddTransient<ITooltipService, TooltipService>();

        return services;
    }
}
