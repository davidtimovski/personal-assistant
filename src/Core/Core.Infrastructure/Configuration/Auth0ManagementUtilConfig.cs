using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class Auth0ManagementUtilConfig
{
    [Required]
    public string Domain { get; } = null!;

    [Required]
    public string ClientId { get; } = null!;

    [Required]
    public string ClientSecret { get; } = null!;
}
