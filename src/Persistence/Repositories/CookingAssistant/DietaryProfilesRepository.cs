using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
    public class DietaryProfilesRepository : BaseRepository, IDietaryProfilesRepository
    {
        public DietaryProfilesRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public DietaryProfile Get(int userId)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT dp.*, u.""Id"", u.""ImperialSystem""
                        FROM ""CookingAssistant.DietaryProfiles"" AS dp
                        INNER JOIN ""AspNetUsers"" AS u ON dp.""UserId"" = u.""Id""
                        WHERE dp.""UserId"" = @UserId";

            var dietaryProfiles = conn.Query<DietaryProfile, User, DietaryProfile>(sql,
                (detaryProfile, user) =>
                {
                    detaryProfile.User = user;
                    return detaryProfile;
                }, new { UserId = userId }, null, true);

            if (!dietaryProfiles.Any())
            {
                return null;
            }

            return dietaryProfiles.Single();
        }

        public async Task CreateOrUpdateAsync(DietaryProfile dietaryProfile)
        {
            var now = DateTime.UtcNow;

            DietaryProfile dbDietaryProfile = EFContext.DietaryProfiles.Find(dietaryProfile.UserId);

            if (dbDietaryProfile == null)
            {
                dietaryProfile.CreatedDate = now;
                dietaryProfile.ModifiedDate = now;
                EFContext.DietaryProfiles.Add(dietaryProfile);
            }
            else
            {
                dbDietaryProfile.Birthday = dietaryProfile.Birthday;
                dbDietaryProfile.Gender = dietaryProfile.Gender;
                dbDietaryProfile.Height = dietaryProfile.Height;
                dbDietaryProfile.Weight = dietaryProfile.Weight;
                dbDietaryProfile.ActivityLevel = dietaryProfile.ActivityLevel;
                dbDietaryProfile.Goal = dietaryProfile.Goal;
                dbDietaryProfile.CustomCalories = dietaryProfile.CustomCalories;
                dbDietaryProfile.TrackCalories = dietaryProfile.TrackCalories;
                dbDietaryProfile.CustomSaturatedFat = dietaryProfile.CustomSaturatedFat;
                dbDietaryProfile.TrackSaturatedFat = dietaryProfile.TrackSaturatedFat;
                dbDietaryProfile.CustomCarbohydrate = dietaryProfile.CustomCarbohydrate;
                dbDietaryProfile.TrackCarbohydrate = dietaryProfile.TrackCarbohydrate;
                dbDietaryProfile.CustomAddedSugars = dietaryProfile.CustomAddedSugars;
                dbDietaryProfile.TrackAddedSugars = dietaryProfile.TrackAddedSugars;
                dbDietaryProfile.CustomFiber = dietaryProfile.CustomFiber;
                dbDietaryProfile.TrackFiber = dietaryProfile.TrackFiber;
                dbDietaryProfile.CustomProtein = dietaryProfile.CustomProtein;
                dbDietaryProfile.TrackProtein = dietaryProfile.TrackProtein;
                dbDietaryProfile.CustomSodium = dietaryProfile.CustomSodium;
                dbDietaryProfile.TrackSodium = dietaryProfile.TrackSodium;
                dbDietaryProfile.CustomCholesterol = dietaryProfile.CustomCholesterol;
                dbDietaryProfile.TrackCholesterol = dietaryProfile.TrackCholesterol;
                dbDietaryProfile.CustomVitaminA = dietaryProfile.CustomVitaminA;
                dbDietaryProfile.TrackVitaminA = dietaryProfile.TrackVitaminA;
                dbDietaryProfile.CustomVitaminC = dietaryProfile.CustomVitaminC;
                dbDietaryProfile.TrackVitaminC = dietaryProfile.TrackVitaminC;
                dbDietaryProfile.CustomVitaminD = dietaryProfile.CustomVitaminD;
                dbDietaryProfile.TrackVitaminD = dietaryProfile.TrackVitaminD;
                dbDietaryProfile.CustomCalcium = dietaryProfile.CustomCalcium;
                dbDietaryProfile.TrackCalcium = dietaryProfile.TrackCalcium;
                dbDietaryProfile.CustomIron = dietaryProfile.CustomIron;
                dbDietaryProfile.TrackIron = dietaryProfile.TrackIron;
                dbDietaryProfile.CustomPotassium = dietaryProfile.CustomPotassium;
                dbDietaryProfile.TrackPotassium = dietaryProfile.TrackPotassium;
                dbDietaryProfile.CustomMagnesium = dietaryProfile.CustomMagnesium;
                dbDietaryProfile.TrackMagnesium = dietaryProfile.TrackMagnesium;
                dbDietaryProfile.ModifiedDate = now;
            }

            await EFContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            DietaryProfile dietaryProfile = EFContext.DietaryProfiles.First(x => x.UserId == userId);
            EFContext.DietaryProfiles.Remove(dietaryProfile);

            await EFContext.SaveChangesAsync();
        }
    }
}
