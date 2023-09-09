using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class KeyVaultConfiguration
{
    [Required]
    public required string Url { get; set; }

    [Required]
    public required string TenantId { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}
