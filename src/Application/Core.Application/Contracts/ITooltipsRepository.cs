﻿using Application.Domain.Common;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipsRepository
{
    IEnumerable<Tooltip> GetAll(string application, int userId, ISpan metricsSpan);
    Tooltip GetByKey(int userId, string key, string application, ISpan metricsSpan);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan);
}
