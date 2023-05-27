﻿using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Core.Application.Contracts;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Sender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Core.Infrastructure;

public static class IoC
{
    public static IServiceCollection AddAuth0(this IServiceCollection services, string authority, string audience, string signalrHub = null)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.Authority = authority;
                opt.Audience = audience;

                // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };

                if (signalrHub != null)
                {
                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments(signalrHub)))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                }
            });

        AddUserIdMapper(services);

        return services;
    }

    public static IHostBuilder AddKeyVault(
        this IHostBuilder host,
        Uri keyVaultUri,
        string tenantId,
        string clientId,
        string clientSecret)
    {
        var tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var secretClient = new SecretClient(keyVaultUri, tokenCredential);

        host.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
        });

        return host;
    }

    public static IHostBuilder AddSentryLogging(
        this IHostBuilder host,
        string dsn,
        HashSet<string> excludeTransactions)
    {
        host.ConfigureLogging((context, loggingBuilder) =>
        {
            loggingBuilder.AddSentry(opt =>
            {
                opt.Dsn = dsn;
                opt.SampleRate = 1;
                opt.TracesSampler = samplingCtx =>
                {
                    if (samplingCtx?.TransactionContext == null)
                    {
                        return 1;
                    }

                    if (excludeTransactions.Contains(samplingCtx.TransactionContext.Name))
                    {
                        return 0;
                    }

                    return 1;
                };
            });
        });

        return host;
    }

    public static IServiceCollection AddDataProtectionWithCertificate(
        this IServiceCollection services,
        Uri keyVaultUri,
        string tenantId,
        string clientId,
        string clientSecret)
    {
        var tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        var certClient = new CertificateClient(keyVaultUri, tokenCredential);
        X509Certificate2 certificate = certClient.DownloadCertificate("personal-assistant");

        services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("storage/dataprotection-keys/"))
            .ProtectKeysWithCertificate(certificate);

        return services;
    }

    public static IServiceCollection AddUserIdMapper(this IServiceCollection services)
    {
        services.AddSingleton<IUserIdLookup, UserIdLookup>();

        return services;
    }

    public static IServiceCollection AddSender(this IServiceCollection services)
    {
        services.AddSingleton<ISenderService, SenderService>();

        return services;
    }
}