using System.ComponentModel.DataAnnotations;

namespace CookingAssistant.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
