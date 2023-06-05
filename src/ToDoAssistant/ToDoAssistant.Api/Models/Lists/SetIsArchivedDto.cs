using System.Text.Json.Serialization;

namespace ToDoAssistant.Api.Models.Lists;

public class SetIsArchivedDto
{
    [JsonRequired]
    public int ListId { get; set; }

    [JsonRequired]
    public bool IsArchived { get; set; }
}
