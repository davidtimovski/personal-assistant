using Accountant.Application.Contracts.Sync.Models;

namespace Accountant.Application.Contracts.Sync;

public interface ISyncService
{
    Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model);
}
