using System.ComponentModel.DataAnnotations;

namespace Weatherman.Persistence;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
