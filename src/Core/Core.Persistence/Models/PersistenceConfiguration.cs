using System.ComponentModel.DataAnnotations;

namespace Core.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public required string ConnectionString { get; set; }
}
