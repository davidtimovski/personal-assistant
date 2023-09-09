using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace PersonalAssistant.Web.Models;

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
    /// Coming from appsettings.json.
    /// </summary>
    [Required]
    public required string ReCaptchaVerificationUrl { get; set; }

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public required AppUrls Urls { get; set; }

    /// <summary>
    /// Coming from environment variables and Azure Key Vault.
    /// </summary>
    [Required]
    public required string AdminEmail { get; set; }

    /// <summary>
    /// Coming from appsettings.*.json and Azure Key Vault.
    /// </summary>
    [Required]
    public required Auth0ManagementUtilConfig Auth0 { get; set; }

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
    public required AppSecrets PersonalAssistant { get; set; }
}

public class AppUrls
{
    [Required]
    public required string PersonalAssistant { get; set; }

    [Required]
    public required string ToDoAssistant { get; set; }

    [Required]
    public required string CookingAssistant { get; set; }

    [Required]
    public required string Accountant { get; set; }

    [Required]
    public required string Weatherman { get; set; }
}

public class AppSecrets
{
    [Required]
    public required string ReCaptchaSecret { get; set; }

    [Required]
    public required SentryConfiguration Sentry { get; set; }
}
