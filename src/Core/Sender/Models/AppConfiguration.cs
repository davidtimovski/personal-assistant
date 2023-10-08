using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Sender.Models;

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
    public required string SystemEmail { get; set; }

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public required SenderConfiguration RabbitMQ { get; set; }

    /// <summary>
    /// Coming from environment variables and Azure Key Vault.
    /// </summary>
    [Required]
    public required string SendGridApiKey { get; set; }

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
    [Required]
    public required AppSecrets Sender { get; set; }
}

public class VapidConfiguration
{
    [Required]
    public required string PublicKey { get; set; }

    [Required]
    public required string PrivateKey { get; set; }
}

public class AppSecrets
{
    [Required]
    public required VapidConfiguration ToDoAssistantVapid { get; set; }

    [Required]
    public required VapidConfiguration ChefVapid { get; set; }
}
