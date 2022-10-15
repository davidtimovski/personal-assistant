using System;

namespace Infrastructure.Identity;

public class ApplicationUser
{
    public string Name { get; set; }
    public string Language { get; set; }
    public bool ToDoNotificationsEnabled { get; set; }
    public bool CookingNotificationsEnabled { get; set; }
    public bool ImperialSystem { get; set; }
    public string ImageUri { get; set; }
}
