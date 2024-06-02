using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Sender.Models;

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
    public required string SystemEmail { get; init; }

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public required SenderConfiguration RabbitMQ { get; init; }

    /// <summary>
    /// Coming from environment variables and Azure Key Vault.
    /// </summary>
    [Required]
    public required string SendGridApiKey { get; init; }

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
    [Required]
    public required AppSecrets Sender { get; init; }
}

public sealed class VapidConfiguration
{
    [Required]
    public required string PublicKey { get; init; }

    [Required]
    public required string PrivateKey { get; init; }
}

public sealed class AppSecrets
{
    [Required]
    public required VapidConfiguration ToDoAssistantVapid { get; init; }

    [Required]
    public required VapidConfiguration ChefVapid { get; init; }
}
