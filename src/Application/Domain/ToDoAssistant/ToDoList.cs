using Application.Domain.Common;

namespace Application.Domain.ToDoAssistant;

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
    public List<ListShare> Shares { get; set; } = new List<ListShare>();

    public bool IsShared
    {
        get
        {
            return Shares.Any(x => x.IsAccepted == true);
        }
    }
}