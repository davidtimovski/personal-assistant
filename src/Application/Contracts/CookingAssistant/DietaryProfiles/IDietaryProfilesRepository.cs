using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles
{
    public interface IDietaryProfilesRepository
    {
        Task<DietaryProfile> GetAsync(int userId);
        Task UpdateAsync(DietaryProfile dietaryProfile);
        Task DeleteAsync(int userId);
    }
}
