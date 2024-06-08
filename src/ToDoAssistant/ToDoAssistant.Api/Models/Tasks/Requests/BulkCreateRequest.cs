namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record BulkCreateRequest(int ListId, string TasksText, bool TasksAreOneTime, bool TasksArePrivate);
