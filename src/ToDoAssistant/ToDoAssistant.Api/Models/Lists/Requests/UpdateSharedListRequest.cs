namespace ToDoAssistant.Api.Models.Lists.Requests;

public record UpdateSharedListRequest(int Id, bool NotificationsEnabled);
