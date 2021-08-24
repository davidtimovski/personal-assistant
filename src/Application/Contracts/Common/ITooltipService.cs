using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ITooltipService
    {
        IEnumerable<TooltipDto> GetAll(string application, int userId);
        TooltipDto GetByKey(int userId, string key, string application);
        Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
    }
}
