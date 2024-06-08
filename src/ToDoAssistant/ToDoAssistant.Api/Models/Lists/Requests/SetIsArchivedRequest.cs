namespace ToDoAssistant.Api.Models.Lists.Requests;

public record SetIsArchivedRequest(int ListId, bool IsArchived);
