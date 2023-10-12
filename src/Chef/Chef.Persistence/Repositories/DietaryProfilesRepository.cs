using System.Data;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Entities;
using Dapper;
using Sentry;
using User = Chef.Application.Entities.User;

namespace Chef.Persistence.Repositories;

public class DietaryProfilesRepository : BaseRepository, IDietaryProfilesRepository
{
    public DietaryProfilesRepository(ChefContext efContext)
        : base(efContext) { }

    public DietaryProfile? Get(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfilesRepository)}.{nameof(Get)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT dp.*, u.id, u.imperial_system
                               FROM chef.dietary_profiles AS dp
                               INNER JOIN users AS u ON dp.user_id = u.id
                               WHERE dp.user_id = @UserId";

        var dietaryProfiles = conn.Query<DietaryProfile, User, DietaryProfile>(query,
            (dietaryProfile, user) =>
            {
                dietaryProfile.User = user;
                return dietaryProfile;
            }, new { UserId = userId });

        if (!dietaryProfiles.Any())
        {
            return null;
        }

        var result = dietaryProfiles.Single();

        metric.Finish();

        return result;
    }

    public async Task CreateOrUpdateAsync(DietaryProfile dietaryProfile, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfilesRepository)}.{nameof(CreateOrUpdateAsync)}");

        var now = DateTime.UtcNow;

        var dbDietaryProfile = EFContext.DietaryProfiles.Find(dietaryProfile.UserId);

        if (dbDietaryProfile is null)
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

        metric.Finish();
    }

    public async Task DeleteAsync(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfilesRepository)}.{nameof(DeleteAsync)}");

        DietaryProfile dietaryProfile = EFContext.DietaryProfiles.First(x => x.UserId == userId);
        EFContext.DietaryProfiles.Remove(dietaryProfile);

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }
}
