using Domain.Entities.Common;

namespace Domain.Entities.ToDoAssistant
{
    public class ListShare : Entity
    {
        public int ListId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsAccepted { get; set; }
        public short? Order { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool IsArchived { get; set; }

        public ToDoList List { get; set; }
        public User User { get; set; }
    }
}
