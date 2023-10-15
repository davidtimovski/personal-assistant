using Core.Application.Contracts;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class ReorderTaskResult : IResult
{
    public ReorderTaskResult()
    {
        Failed = true;
    }

    public ReorderTaskResult(bool success)
    {
        Failed = !success;
    }

    public bool Failed { get; private set; }

    public ReorderTaskResult(int listId, bool notifySignalR)
    {
        ListId = listId;
        NotifySignalR = notifySignalR;
    }

    public int ListId { get; }
    public bool NotifySignalR { get; }
}
