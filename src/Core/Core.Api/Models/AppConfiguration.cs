using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Core.Api.Models;

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
    /// Coming from Azure Key Vault.
    /// </summary>
#if !DEBUG
    [Required]
#endif
    public required AppSecrets Core { get; init; }
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
