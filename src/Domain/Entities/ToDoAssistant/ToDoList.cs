using System.Collections.Generic;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Domain.Entities.ToDoAssistant
{
    public class ToDoList : Entity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public short? Order { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool IsOneTimeToggleDefault { get; set; }
        public bool IsArchived { get; set; }

        public User User { get; set; }
        public List<ToDoTask> Tasks { get; set; } = new List<ToDoTask>();
        public List<Share> Shares { get; set; } = new List<Share>();
    }
}
