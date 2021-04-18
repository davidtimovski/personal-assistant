using System.Collections.Generic;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Domain.Entities.ToDoAssistant
{
    public class ToDoTask : Entity
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOneTime { get; set; }
        public int? PrivateToUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public short Order { get; set; }

        public ToDoList List { get; set; }
        public User AssignedToUser { get; set; }
    }
}
