using System.Text.Json.Serialization;

namespace Core.Infrastructure.Identity;

internal class TokenResult
{
    [JsonRequired]
    public string access_token { get; set; } = null!;

    [JsonRequired]
    public int expires_in { get; set; }

    [JsonRequired]
    public string scope { get; set; } = null!;

    [JsonRequired]
    public string token_type { get; set; } = null!;
}
