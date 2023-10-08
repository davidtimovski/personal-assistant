using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

public class TokenResult
{
    [JsonRequired]
    public required string access_token { get; init; }

    [JsonRequired]
    public required int expires_in { get; init; }

    [JsonRequired]
    public required string scope { get; init; }

    [JsonRequired]
    public required string token_type { get; init; }
}
