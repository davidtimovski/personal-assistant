using Core.Application.Contracts.Models.Sender;

namespace Chef.Application.Contracts;

public class ChefPushNotification : PushNotification
{
    public ChefPushNotification() : base("Chef") { }
}
