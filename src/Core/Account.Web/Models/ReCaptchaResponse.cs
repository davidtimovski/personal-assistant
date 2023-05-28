using System.Text.Json.Serialization;

namespace Account.Web.Models;

public class ReCaptchaResponse
{
    [JsonRequired]
    public float Score { get; set; }
}
