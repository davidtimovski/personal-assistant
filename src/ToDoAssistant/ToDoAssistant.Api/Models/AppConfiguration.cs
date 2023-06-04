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
    public KeyVaultConfiguration KeyVault { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.json and Azure Key Vault.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public Auth0Config Auth0 { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.json and environment variables.
    /// </summary>
    [Required]
    public CloudinaryConfig Cloudinary { get; set; } = null!;

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public SenderConfiguration RabbitMQ { get; set; } = null!;

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public AppSecrets ToDoAssistant { get; set; } = null!;
}

public class Auth0Config
{
    [Required]
    public string Domain { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;
}

public class AppSecrets
{
    [Required]
    public SentryConfiguration Sentry { get; set; } = null!;
}
