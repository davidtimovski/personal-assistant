using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Common.Models;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ITooltipService
    {
        Task<IEnumerable<TooltipDto>> GetAllAsync(string application, int userId);
        Task<TooltipDto> GetByKeyAsync(int userId, string key);
        Task ToggleDismissedAsync(int userId, string key, bool isDismissed);
    }
}
