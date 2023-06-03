using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;

namespace Core.Api.Models;

public class AppConfiguration
{
#if !DEBUG
    [Required]
#endif
    public KeyVaultConfiguration KeyVault { get; set; } = null!;

    /// <summary>
    /// Used in persistence.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Used by Auth0.
    /// </summary>
    [Required]
    public Auth0Config Auth0 { get; set; } = null!;
}

public class Auth0Config
{
    [Required]
    public string Domain { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;
}
