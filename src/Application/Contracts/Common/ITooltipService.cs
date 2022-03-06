using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Common.Models;

namespace Application.Contracts.Common;

public interface ITooltipService
{
    IEnumerable<TooltipDto> GetAll(string application, int userId);
    TooltipDto GetByKey(int userId, string key, string application);
    Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
}
