using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Core.Application.Contracts;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Sender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.Infrastructure;

public static class IoC
{
    public static IServiceCollection AddAuth0(
        this IServiceCollection services,
        IConfiguration configuration,
        string? signalrHub = null)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                var config = configuration.GetSection("Auth0").Get<Auth0Configuration>();
                if (config is null)
                {
                    throw new ArgumentNullException("Auth0 configuration is missing");
                }

                opt.Authority = config.Authority;
                opt.Audience = config.Audience;

                // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`.
                // Map it to a different claim by setting the NameClaimType below.
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

    public static IHostBuilder AddKeyVault(this IHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var config = configBuilder.Build();

            var keyVaultConfig = config.GetSection("KeyVault").Get<KeyVaultConfiguration>();
            if (keyVaultConfig is null)
            {
                throw new ArgumentNullException("KeyVault configuration is missing");
            }

            var tokenCredential = new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecret);
            var secretClient = new SecretClient(new Uri(keyVaultConfig.Url), tokenCredential);

            configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
        });

        return host;
    }

    public static IHostBuilder AddSentryLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string configSection,
        HashSet<string> excludeTransactions)
    {
        hostBuilder.ConfigureLogging((context, loggingBuilder) =>
        {
            var config = configuration.GetSection($"{configSection}:Sentry").Get<SentryConfiguration>();
            if (config is null)
            {
                throw new ArgumentNullException($"{configSection}:Sentry configuration is missing");
            }

            loggingBuilder.AddSentry(opt =>
            {
                opt.Dsn = config.Dsn;
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

        return hostBuilder;
    }

    public static IServiceCollection AddDataProtectionWithCertificate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keyVaultConfig = configuration.GetSection("KeyVault").Get<KeyVaultConfiguration>();
        if (keyVaultConfig is null)
        {
            throw new ArgumentNullException("KeyVault configuration is missing");
        }

        var tokenCredential = new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecret);

        var certClient = new CertificateClient(new Uri(keyVaultConfig.Url), tokenCredential);
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

    public static IServiceCollection AddSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SenderConfiguration>()
            .Bind(configuration.GetSection("RabbitMQ"))
            .ValidateDataAnnotations();

        services.AddSingleton<ISenderService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<SenderConfiguration>>().Value;
            return new SenderService(config);
        });

        return services;
    }
}
