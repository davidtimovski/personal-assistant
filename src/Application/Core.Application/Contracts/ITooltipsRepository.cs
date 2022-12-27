using Application.Domain.Common;

namespace Core.Application.Contracts;

public interface ITooltipsRepository
{
    IEnumerable<Tooltip> GetAll(string application, int userId);
    Tooltip GetByKey(int userId, string key, string application);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
}
