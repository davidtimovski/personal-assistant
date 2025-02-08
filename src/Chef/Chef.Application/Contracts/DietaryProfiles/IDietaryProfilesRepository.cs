using Chef.Application.Entities;

namespace Chef.Application.Contracts.DietaryProfiles;

public interface IDietaryProfilesRepository
{
    DietaryProfile? Get(int userId, ISpan metricsSpan);
    Task CreateOrUpdateAsync(DietaryProfile dietaryProfile, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken);
}
