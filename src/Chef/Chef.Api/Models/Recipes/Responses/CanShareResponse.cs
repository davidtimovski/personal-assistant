namespace Chef.Api.Models.Recipes.Responses;

public class CanShareResponse
{
    public required bool CanShare { get; set; }
    public int? UserId { get; set; }
    public string? ImageUri { get; set; }
}
