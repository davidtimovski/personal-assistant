namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record ReorderTaskRequest(int Id, short OldOrder, short NewOrder);
