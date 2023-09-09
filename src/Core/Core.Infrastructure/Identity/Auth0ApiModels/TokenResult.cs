using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

public class TokenResult
{
    [JsonRequired]
    public required string access_token { get; set; }

    [JsonRequired]
    public required int expires_in { get; set; }

    [JsonRequired]
    public required string scope { get; set; }

    [JsonRequired]
    public required string token_type { get; set; }
}
