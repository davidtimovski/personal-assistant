using System.Threading.Tasks;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.DietaryProfiles
{
    public interface IDietaryProfilesRepository
    {
        DietaryProfile Get(int userId);
        Task CreateOrUpdateAsync(DietaryProfile dietaryProfile);
        Task DeleteAsync(int userId);
    }
}
