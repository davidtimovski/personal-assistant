using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles
{
    public interface IDietaryProfileService
    {
        Task<EditDietaryProfile> GetAsync(int userId);
        RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator);
        Task UpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator);
        Task DeleteAsync(int userId);
    }
}
