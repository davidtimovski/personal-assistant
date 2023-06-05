using System.Text.Json.Serialization;

namespace ToDoAssistant.Api.Models.Lists;

public class SetTasksAsNotCompletedDto
{
    [JsonRequired]
    public int ListId { get; set; }
}
