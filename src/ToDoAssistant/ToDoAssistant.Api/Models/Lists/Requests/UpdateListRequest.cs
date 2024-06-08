namespace ToDoAssistant.Api.Models.Lists.Requests;

public record UpdateListRequest(int Id, string Name, string Icon, bool IsOneTimeToggleDefault, bool NotificationsEnabled);
