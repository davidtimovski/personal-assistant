using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class Auth0Configuration
{
    [Required]
    public required string Domain { get; set; }

    [Required]
    public required string Audience { get; set; }

    public string Authority
    {
        get
        {
            return $"https://{Domain}/";
        }
    }
}
