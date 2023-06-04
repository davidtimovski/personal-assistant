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
    public KeyVaultConfiguration KeyVault { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.json and Azure Key Vault.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.json.
    /// </summary>
    [Required]
    public string ReCaptchaVerificationUrl { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public AppUrls Urls { get; set; } = null!;

    /// <summary>
    /// Coming from environment variables and Azure Key Vault.
    /// </summary>
    [Required]
    public string AdminEmail { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json and Azure Key Vault.
    /// </summary>
    [Required]
    public Auth0ManagementUtilConfig Auth0 { get; set; } = null!;

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
    public AppSecrets PersonalAssistant { get; set; } = null!;
}

public class AppUrls
{
    [Required]
    public string PersonalAssistant { get; set; } = null!;

    [Required]
    public string ToDoAssistant { get; set; } = null!;

    [Required]
    public string CookingAssistant { get; set; } = null!;

    [Required]
    public string Accountant { get; set; } = null!;

    [Required]
    public string Weatherman { get; set; } = null!;
}

public class AppSecrets
{
    [Required]
    public string ReCaptchaSecret { get; set; } = null!;

    [Required]
    public SentryConfiguration Sentry { get; set; } = null!;
}
