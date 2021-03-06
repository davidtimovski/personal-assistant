﻿using System.Threading.Tasks;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface IPushSubscriptionService
    {
        Task CreateSubscriptionAsync(int userId, string application, string endpoint, string authKey, string p256dhKey);
    }
}
