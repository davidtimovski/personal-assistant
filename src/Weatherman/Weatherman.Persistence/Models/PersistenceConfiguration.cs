using System.ComponentModel.DataAnnotations;

namespace Weatherman.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
