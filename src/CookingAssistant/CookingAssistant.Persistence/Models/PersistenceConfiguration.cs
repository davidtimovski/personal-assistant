using System.ComponentModel.DataAnnotations;

namespace CookingAssistant.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public required string ConnectionString { get; set; }
}
