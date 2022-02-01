using System.Threading.Tasks;
using Application.Contracts.Accountant.Sync.Models;

namespace Application.Contracts.Accountant.Sync
{
    public interface ISyncService
    {
        Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model);
    }
}
