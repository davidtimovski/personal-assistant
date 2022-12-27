using Application.Domain.CookingAssistant;

namespace CookingAssistant.Application.Contracts.DietaryProfiles;

public interface IDietaryProfilesRepository
{
    DietaryProfile Get(int userId);
    Task CreateOrUpdateAsync(DietaryProfile dietaryProfile);
    Task DeleteAsync(int userId);
}
