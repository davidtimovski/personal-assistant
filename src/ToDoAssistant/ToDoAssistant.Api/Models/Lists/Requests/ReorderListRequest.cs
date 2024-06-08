namespace ToDoAssistant.Api.Models.Lists.Requests;

public record ReorderListRequest(int Id, short OldOrder, short NewOrder);
