using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace PersonalAssistant.Web.Models;

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
    /// Coming from appsettings.json.
    /// </summary>
    [Required]
    public required string ReCaptchaVerificationUrl { get; init; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required AppUrls Urls { get; init; }

    /// <summary>
    /// Coming from environment variables and Azure Key Vault.
    /// </summary>
    [Required]
    public required string AdminEmail { get; init; }

    /// <summary>
    /// Coming from appsettings.*.json and Azure Key Vault.
    /// </summary>
    [Required]
    public required Auth0ManagementUtilConfig Auth0 { get; init; }

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
    public required AppSecrets PersonalAssistant { get; init; }
}

public sealed class AppUrls
{
    [Required]
    public required string PersonalAssistant { get; init; }

    [Required]
    public required string ToDoAssistant { get; init; }

    [Required]
    public required string Chef { get; init; }

    [Required]
    public required string Accountant { get; init; }

    [Required]
    public required string Weatherman { get; init; }
}

public sealed class AppSecrets
{
    [Required]
    public required string ReCaptchaSecret { get; init; }

    [Required]
    public required SentryConfiguration Sentry { get; init; }
}
