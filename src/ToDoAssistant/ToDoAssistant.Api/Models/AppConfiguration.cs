using System.ComponentModel.DataAnnotations;
using Cdn.Configuration;
using Core.Infrastructure.Configuration;

namespace ToDoAssistant.Api.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.Production.json and environment variables.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required KeyVaultConfiguration KeyVault { get; set; }

    /// <summary>
    /// Coming from appsettings.json and Azure Key Vault.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required Auth0Config Auth0 { get; set; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required string Url { get; set; }

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public required CloudinaryConfig Cloudinary { get; set; }

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public required SenderConfiguration RabbitMQ { get; set; }

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required AppSecrets ToDoAssistant { get; set; }
}

public class Auth0Config
{
    [Required]
    public required string Domain { get; set; }

    [Required]
    public required string Audience { get; set; }
}

public class AppSecrets
{
    [Required]
    public required SentryConfiguration Sentry { get; set; }
}
