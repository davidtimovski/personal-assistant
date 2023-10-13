using Core.Application.Contracts.Models;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipService
{
    IEnumerable<TooltipDto> GetAll(string application, int userId, ISpan metricsSpan);
    TooltipDto GetByKey(int userId, string key, string application, ISpan metricsSpan);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken);
}
