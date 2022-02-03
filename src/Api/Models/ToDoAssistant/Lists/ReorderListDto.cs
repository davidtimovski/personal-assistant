namespace Api.Models.ToDoAssistant.Lists;

public class ReorderListDto
{
    public int Id { get; set; }
    public short OldOrder { get; set; }
    public short NewOrder { get; set; }
}