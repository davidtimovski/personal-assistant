namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record CreateTaskRequest(int ListId, string Name, string? Url, bool IsOneTime, bool? IsPrivate);
