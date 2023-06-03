using System.ComponentModel.DataAnnotations;

namespace Core.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
