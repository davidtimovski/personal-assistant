using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class KeyVaultConfiguration
{
    [Required]
    public string Url { get; set; } = null!;

    [Required]
    public string TenantId { get; set; } = null!;

    [Required]
    public string ClientId { get; set; } = null!;

    [Required]
    public string ClientSecret { get; set; } = null!;
}
