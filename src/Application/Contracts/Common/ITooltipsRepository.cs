using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Application.Contracts.Common;

public interface ITooltipsRepository
{
    IEnumerable<Tooltip> GetAll(string application, int userId);
    Tooltip GetByKey(int userId, string key, string application);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
}
