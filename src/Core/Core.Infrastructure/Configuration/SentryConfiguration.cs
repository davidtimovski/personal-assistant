using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

public class SentryConfiguration
{
    [Required]
    public required string Dsn { get; set; }
}
