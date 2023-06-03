using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class Auth0Configuration
{
    [Required]
    public string Domain { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    public string Authority
    {
        get
        {
            return $"https://{Domain}/";
        }
    }
}
