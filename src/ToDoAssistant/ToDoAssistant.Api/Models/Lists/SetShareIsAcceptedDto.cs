using System.Text.Json.Serialization;

namespace ToDoAssistant.Api.Models.Lists;

public class SetShareIsAcceptedDto
{
    [JsonRequired]
    public int ListId { get; set; }

    [JsonRequired]
    public bool IsAccepted { get; set; }
}
