namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record UpdateTaskRequest(int Id, int ListId, string Name, string? Url, bool IsOneTime, bool IsHighPriority, bool? IsPrivate, int? AssignedToUserId);
