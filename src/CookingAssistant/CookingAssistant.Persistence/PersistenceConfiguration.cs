using System.ComponentModel.DataAnnotations;

namespace CookingAssistant.Persistence;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
