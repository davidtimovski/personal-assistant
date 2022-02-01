namespace Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class ReorderTaskResult
    {
        public ReorderTaskResult(int listId, bool notifySignalR)
        {
            ListId = listId;
            NotifySignalR = notifySignalR;
        }

        public int ListId { get; }
        public bool NotifySignalR { get; }
    }
}
