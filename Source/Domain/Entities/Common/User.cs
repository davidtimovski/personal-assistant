using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Domain.Entities.Common
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public bool ToDoNotificationsEnabled { get; set; }
        public bool CookingNotificationsEnabled { get; set; }
        public bool ImperialSystem { get; set; }
        public string ImageUri { get; set; }

        public DietaryProfile DietaryProfile { get; set; }
    }
}
