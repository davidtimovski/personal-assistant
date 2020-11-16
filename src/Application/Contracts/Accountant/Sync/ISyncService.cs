using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Sync.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Common
{
    public interface ISyncService
    {
        Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model);
    }
}
