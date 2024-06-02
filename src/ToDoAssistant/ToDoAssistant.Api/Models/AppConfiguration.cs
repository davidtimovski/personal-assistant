using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;
using Core.Infrastructure.Configuration;

namespace ToDoAssistant.Api.Models;

public sealed class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.Production.json and environment variables.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required KeyVaultConfiguration KeyVault { get; init; }

    /// <summary>
    /// Coming from appsettings.json and Azure Key Vault.
    /// </summary>
    [Required]
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required Auth0Config Auth0 { get; init; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required string Url { get; init; }

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public required CloudinaryConfig Cloudinary { get; init; }

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public required SenderConfiguration RabbitMQ { get; init; }

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required AppSecrets ToDoAssistant { get; init; }
}

public sealed class Auth0Config
{
    [Required]
    public required string Domain { get; init; }

    [Required]
    public required string Audience { get; init; }
}

public sealed class AppSecrets
{
    [Required]
    public required SentryConfiguration Sentry { get; init; }
}
