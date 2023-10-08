using Chef.Application.Contracts.Common;

namespace Chef.Application.Services;

public class DailyIntakeHelper : IDailyIntakeHelper
{
    private readonly IReadOnlyDictionary<string, float> _activityMultiplier;
    private readonly IReadOnlyDictionary<string, short> _dietaryGoalCalories;

    public DailyIntakeHelper(Dictionary<string, float> activityMultiplier, Dictionary<string, short> dietaryGoalCalories)
    {
        _activityMultiplier = activityMultiplier;
        _dietaryGoalCalories = dietaryGoalCalories;
    }

    public short DeriveDailyCaloriesIntake(short age, string gender, float height, float weight, string activityLevel, string goal)
    {
        // Find BMR (Mifflin-St Jeor equation)
        double bmr = Math.Round(10 * weight + 6.25 * height - 5 * age);
        if (gender == "Male")
        {
            bmr += 5;
        }
        else
        {
            bmr -= 161;
        }

        // Find recommended daily calories based on activity level
        bmr = Math.Round(bmr * _activityMultiplier[activityLevel]);

        // Adjust recommended daily calories based on dietary goal
        bmr = Math.Round(bmr + _dietaryGoalCalories[goal]);

        return (short)bmr;
    }
}
