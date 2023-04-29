using Core.Application.Contracts.Models;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipService
{
    IEnumerable<TooltipDto> GetAll(string application, int userId, ITransaction tr);
    TooltipDto GetByKey(int userId, string key, string application, ITransaction tr);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ITransaction tr);
}
