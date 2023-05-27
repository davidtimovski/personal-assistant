public class PushNotificationsSubscription
{
    public string Application { get; set; }
    public Subscription Subscription { get; set; }
}

public class Subscription
{
    public string Endpoint { get; set; }
    public Dictionary<string, string> Keys { get; set; }
}
