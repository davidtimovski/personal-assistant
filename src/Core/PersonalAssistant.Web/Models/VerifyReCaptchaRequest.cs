using System.Text.Json.Serialization;

namespace PersonalAssistant.Web.Models;

public sealed class VerifyReCaptchaRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}
