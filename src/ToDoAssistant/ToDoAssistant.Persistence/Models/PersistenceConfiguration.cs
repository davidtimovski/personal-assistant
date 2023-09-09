using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Persistence.Models;

public class PersistenceConfiguration
{
    [Required]
    public required string ConnectionString { get; set; }
}
