using System.ComponentModel.DataAnnotations;

namespace Weatherman.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public required string ConnectionString { get; set; }
}
