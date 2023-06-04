using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Sender.Models;

public class AppConfiguration
{
    /// <summary>
    /// Coming from appsettings.*.json and Azure Key Vault.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Coming from appsettings.*.json.
    /// </summary>
    [Required]
    public string SystemEmail { get; set; } = null!;

    /// <summary>
    /// Coming from environment variables.
    /// </summary>
    [Required]
    public SenderConfiguration RabbitMQ { get; set; } = null!;

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
    [Required]
    public string SendGridApiKey { get; set; } = null!;

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
    [Required]
    public VapidConfiguration ToDoAssistantVapid { get; set; } = null!;

    /// <summary>
    /// Coming from Azure Key Vault.
    /// </summary>
    [Required]
    public VapidConfiguration CookingAssistantVapid { get; set; } = null!;
}

public class VapidConfiguration
{
    [Required]
    public string PublicKey { get; set; } = null!;

    [Required]
    public string PrivateKey { get; set; } = null!;
}
