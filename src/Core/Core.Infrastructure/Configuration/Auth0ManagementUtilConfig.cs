using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class Auth0ManagementUtilConfig
{
    [Required]
    public required string Domain { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}
