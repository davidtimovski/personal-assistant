using System.Text.Json.Serialization;

namespace PersonalAssistant.Web.Models;

public class ReCaptchaResponse
{
    [JsonRequired]
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }
}
