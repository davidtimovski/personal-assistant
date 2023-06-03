using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
