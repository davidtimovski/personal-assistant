namespace ToDoAssistant.Api.Models.Lists;

public class ReorderListDto
{
    public int Id { get; set; }
    public short OldOrder { get; set; }
    public short NewOrder { get; set; }
}
