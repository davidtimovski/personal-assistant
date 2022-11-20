using Application.Contracts.Models;

namespace Application.Contracts;

public interface ITooltipService
{
    IEnumerable<TooltipDto> GetAll(string application, int userId);
    TooltipDto GetByKey(int userId, string key, string application);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
}
