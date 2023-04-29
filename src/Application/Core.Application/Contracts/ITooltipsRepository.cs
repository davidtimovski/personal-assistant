using Application.Domain.Common;
using Sentry;

namespace Core.Application.Contracts;

public interface ITooltipsRepository
{
    IEnumerable<Tooltip> GetAll(string application, int userId, ITransaction tr);
    Tooltip GetByKey(int userId, string key, string application, ITransaction tr);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ITransaction tr);
}
