namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class ReorderTask
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required short OldOrder { get; init; }
    public required short NewOrder { get; init; }
}
