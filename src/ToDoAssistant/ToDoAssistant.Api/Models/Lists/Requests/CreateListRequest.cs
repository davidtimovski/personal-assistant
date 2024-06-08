namespace ToDoAssistant.Api.Models.Lists.Requests;

public record CreateListRequest(string Name, string Icon, bool IsOneTimeToggleDefault, string? TasksText);
