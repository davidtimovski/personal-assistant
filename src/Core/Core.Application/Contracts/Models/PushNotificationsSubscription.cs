using System.Text.Json.Serialization;

public class PushNotificationsSubscription
{
    [JsonRequired]
    public string Application { get; set; } = null!;

    [JsonRequired]
    public Subscription Subscription { get; set; } = null!;
}

public class Subscription
{
    [JsonRequired]
    public string Endpoint { get; set; } = null!;

    [JsonRequired]
    public Dictionary<string, string> Keys { get; set; } = null!;
}
