using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Account.Web.Models;

public class AppConfiguration
{
#if !DEBUG
    [Required]
#endif
    public KeyVaultConfiguration KeyVault { get; set; } = null!;

    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string ReCaptchaVerificationUrl { get; set; } = null!;

    [Required]
    public AppUrls Urls { get; set; } = null!;

    [Required]
    public string AdminEmail { get; set; } = null!;

    [Required]
    public Auth0ManagementUtilConfig Auth0 { get; set; } = null!;

    [Required]
    public SenderConfiguration RabbitMQ { get; set; } = null!;

#if !DEBUG
    [Required]
#endif
    public AppSecrets Account { get; set; } = null!;
}

public class AppUrls
{
    [Required]
    public string Account { get; set; } = null!;

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
}
