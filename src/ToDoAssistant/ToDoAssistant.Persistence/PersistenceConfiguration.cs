using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Persistence;

public class PersistenceConfiguration
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}
