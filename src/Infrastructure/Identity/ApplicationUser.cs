using System;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string Name { get; set; }
    public string Language { get; set; }
    public bool ToDoNotificationsEnabled { get; set; }
    public bool CookingNotificationsEnabled { get; set; }
    public bool ImperialSystem { get; set; }
    public string ImageUri { get; set; }
    public DateTime DateRegistered { get; set; }
}
