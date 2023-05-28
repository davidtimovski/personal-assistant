using System.ComponentModel.DataAnnotations;

namespace Core.Persistence;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
