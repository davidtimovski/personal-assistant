using System.Text.Json.Serialization;

namespace PersonalAssistant.Web.Models;

public class ReCaptchaResponse
{
    [JsonRequired]
    public float Score { get; set; }
}
