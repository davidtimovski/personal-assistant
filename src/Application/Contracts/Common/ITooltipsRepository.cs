using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ITooltipsRepository
    {
        IEnumerable<Tooltip> GetAll(string application, int userId);
        Tooltip GetByKey(int userId, string key, string application);
        Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed);
    }
}
