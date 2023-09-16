using System.ComponentModel.DataAnnotations;

namespace Chef.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public required string ConnectionString { get; set; }
}
