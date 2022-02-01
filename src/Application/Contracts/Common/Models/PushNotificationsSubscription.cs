using System.Collections.Generic;
using System.Linq;

namespace Application.Contracts.Common.Models
{
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

    //public class PushNotificationsSubscriptionDtoValidator : AbstractValidator<PushNotificationsSubscription>
    //{
    //    private readonly string[] applications = new string[] { "To Do Assistant", "Cooking Assistant" };

    //    public PushNotificationsSubscriptionDtoValidator(IStringLocalizer<PushNotificationsSubscriptionDtoValidator> localizer)
    //    {
    //        RuleFor(dto => dto.Application).NotEmpty().WithMessage(localizer["AnErrorOccurred"]).Must(application => applications.Contains(application)).WithMessage(localizer["AnErrorOccurred"]);
    //        RuleFor(dto => dto.Subscription.Endpoint).NotEmpty().WithMessage(localizer["AnErrorOccurred"]);
    //        RuleFor(dto => dto.Subscription.Keys["auth"]).NotEmpty().WithMessage(localizer["AnErrorOccurred"]);
    //        RuleFor(dto => dto.Subscription.Keys["p256dh"]).NotEmpty().WithMessage(localizer["AnErrorOccurred"]);
    //    }
    //}
}
