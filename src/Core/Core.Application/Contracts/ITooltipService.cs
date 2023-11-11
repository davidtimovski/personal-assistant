using Core.Application.Contracts.Models;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipService
{
    Result<IReadOnlyList<TooltipDto>> GetAll(string application, int userId, ISpan metricsSpan);
    Result<TooltipDto> GetByKey(int userId, string key, string application, ISpan metricsSpan);
    Task<Result> ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken);
}
