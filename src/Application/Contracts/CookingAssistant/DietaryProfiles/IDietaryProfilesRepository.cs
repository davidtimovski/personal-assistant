using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles
{
    public interface IDietaryProfilesRepository
    {
        DietaryProfile Get(int userId);
        Task CreateOrUpdateAsync(DietaryProfile dietaryProfile);
        Task DeleteAsync(int userId);
    }
}
