using System.Text.Json.Serialization;

namespace Chef.Api.Models.Recipes.Responses;

public class CanSendResponse
{
    public required bool CanSend { get; set; }
    public int? UserId { get; set; }
    public string? ImageUri { get; set; }
    public bool? AlreadySent { get; set; }
}
