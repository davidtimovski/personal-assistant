using System.Text.Json.Serialization;

namespace ToDoAssistant.Api.Models.Lists;

public class ReorderListDto
{
    [JsonRequired]
    public int Id { get; set; }

    [JsonRequired]
    public short OldOrder { get; set; }

    [JsonRequired]
    public short NewOrder { get; set; }
}
