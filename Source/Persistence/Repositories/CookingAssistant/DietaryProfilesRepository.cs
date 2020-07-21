using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
    public class DietaryProfilesRepository : BaseRepository, IDietaryProfilesRepository
    {
        public DietaryProfilesRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<DietaryProfile> GetAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT dp.*, u.""Id"", u.""ImperialSystem""
                            FROM ""CookingAssistant.DietaryProfiles"" AS dp
                            INNER JOIN ""AspNetUsers"" AS u ON dp.""UserId"" = u.""Id""
                            WHERE dp.""UserId"" = @UserId";

            var dietaryProfiles = await conn.QueryAsync<DietaryProfile, User, DietaryProfile>(sql,
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

        public async Task UpdateAsync(DietaryProfile dietaryProfile)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var exists = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.DietaryProfiles"" WHERE ""UserId"" = @UserId",
                new { dietaryProfile.UserId });

            var now = DateTime.Now;

            if (exists)
            {
                dietaryProfile.ModifiedDate = now;

                await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.DietaryProfiles"" 
                                              SET ""Birthday"" = @Birthday, ""Gender"" = @Gender, ""Height"" = @Height, ""Weight"" = @Weight, 
                                                ""ActivityLevel"" = @ActivityLevel, ""Goal"" = @Goal, ""CustomCalories"" = @CustomCalories, 
                                                ""TrackCalories"" = @TrackCalories, 
                                                ""CustomSaturatedFat"" = @CustomSaturatedFat, ""TrackSaturatedFat"" = @TrackSaturatedFat, 
                                                ""CustomCarbohydrate"" = @CustomCarbohydrate, ""TrackCarbohydrate"" = @TrackCarbohydrate, 
                                                ""CustomAddedSugars"" = @CustomAddedSugars, ""TrackAddedSugars"" = @TrackAddedSugars,
                                                ""CustomFiber"" = @CustomFiber, ""TrackFiber"" = @TrackFiber, 
                                                ""CustomProtein"" = @CustomProtein, ""TrackProtein"" = @TrackProtein, 
                                                ""CustomSodium"" = @CustomSodium, ""TrackSodium"" = @TrackSodium, 
                                                ""CustomCholesterol"" = @CustomCholesterol, ""TrackCholesterol"" = @TrackCholesterol, 
                                                ""CustomVitaminA"" = @CustomVitaminA, ""TrackVitaminA"" = @TrackVitaminA, 
                                                ""CustomVitaminC"" = @CustomVitaminC, ""TrackVitaminC"" = @TrackVitaminC, 
                                                ""CustomVitaminD"" = @CustomVitaminD, ""TrackVitaminD"" = @TrackVitaminD, 
                                                ""CustomCalcium"" = @CustomCalcium, ""TrackCalcium"" = @TrackCalcium, 
                                                ""CustomIron"" = @CustomIron, ""TrackIron"" = @TrackIron, 
                                                ""CustomPotassium"" = @CustomPotassium, ""TrackPotassium"" = @TrackPotassium, 
                                                ""CustomMagnesium"" = @CustomMagnesium, ""TrackMagnesium"" = @TrackMagnesium, 
                                                ""ModifiedDate"" = @ModifiedDate
                                              WHERE ""UserId"" = @UserId",
                                          dietaryProfile);
            }
            else
            {
                dietaryProfile.CreatedDate = dietaryProfile.ModifiedDate = now;

                await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.DietaryProfiles"" 
                                            (""UserId"", ""Birthday"", ""Gender"", ""Height"", ""Weight"", ""ActivityLevel"", ""Goal"",
                                            ""CustomCalories"", ""TrackCalories"", ""CustomSaturatedFat"", ""TrackSaturatedFat"",
                                            ""CustomCarbohydrate"", ""TrackCarbohydrate"", ""CustomAddedSugars"", ""TrackAddedSugars"",
                                            ""CustomFiber"", ""TrackFiber"", ""CustomProtein"", ""TrackProtein"",
                                            ""CustomSodium"", ""TrackSodium"", ""CustomCholesterol"", ""TrackCholesterol"", 
                                            ""CustomVitaminA"", ""TrackVitaminA"", ""CustomVitaminC"", ""TrackVitaminC"",
                                            ""CustomVitaminD"", ""TrackVitaminD"", ""CustomCalcium"", ""TrackCalcium"",
                                            ""CustomIron"", ""TrackIron"", ""CustomPotassium"", ""TrackPotassium"",
                                            ""CustomMagnesium"", ""TrackMagnesium"", ""CreatedDate"", ""ModifiedDate"")
                                            VALUES (@UserId, @Birthday, @Gender, @Height, @Weight, @ActivityLevel, @Goal,
                                                @CustomCalories, @TrackCalories, @CustomSaturatedFat, @TrackSaturatedFat, 
                                                @CustomCarbohydrate, @TrackCarbohydrate, @CustomAddedSugars, @TrackAddedSugars,
                                                @CustomFiber, @TrackFiber, @CustomProtein, @TrackProtein,
                                                @CustomSodium, @TrackSodium, @CustomCholesterol, @TrackCholesterol,                                                 
                                                @CustomVitaminA, @TrackVitaminA, @CustomVitaminC, @TrackVitaminC,
                                                @CustomVitaminD, @TrackVitaminD, @CustomCalcium, @TrackCalcium,
                                                @CustomIron, @TrackIron, @CustomPotassium, @TrackPotassium,
                                                @CustomMagnesium, @TrackMagnesium, @CreatedDate, @ModifiedDate)",
                                        dietaryProfile);
            }
        }

        public async Task DeleteAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.DietaryProfiles"" WHERE ""UserId"" = @UserId",
                new { UserId = userId });
        }
    }
}
