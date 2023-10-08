using Chef.Application.Entities;

namespace Chef.Application.Contracts.DietaryProfiles;

public interface IDietaryProfilesRepository
{
    DietaryProfile? Get(int userId);
    Task CreateOrUpdateAsync(DietaryProfile dietaryProfile);
    Task DeleteAsync(int userId);
}
