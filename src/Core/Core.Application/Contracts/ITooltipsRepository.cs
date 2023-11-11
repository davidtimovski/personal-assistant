using Core.Application.Entities;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipsRepository
{
    IReadOnlyList<Tooltip> GetAll(string application, int userId, ISpan metricsSpan);
    Tooltip GetByKey(int userId, string key, string application, ISpan metricsSpan);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken);
}
