using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ITooltipsRepository
    {
        Task<IEnumerable<Tooltip>> GetAllAsync(string application, int userId);
        Task<Tooltip> GetByKeyAsync(int userId, string key, string application);
        Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
    }
}
