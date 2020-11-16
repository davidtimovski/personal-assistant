namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class ReorderTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public short OldOrder { get; set; }
        public short NewOrder { get; set; }
    }
}
