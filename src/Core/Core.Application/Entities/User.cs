namespace Core.Application.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Culture { get; set; } = null!;
    public bool ToDoNotificationsEnabled { get; set; }
    public bool ChefNotificationsEnabled { get; set; }
    public bool ImperialSystem { get; set; }
    public string ImageUri { get; set; } = null!;
    public DateTime ModifiedDate { get; set; }
}
