using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class SentryConfiguration
{
    [Required]
    public string Dsn { get; set; } = null!;
}
