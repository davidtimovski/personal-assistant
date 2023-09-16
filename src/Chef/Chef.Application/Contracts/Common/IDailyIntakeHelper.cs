namespace Chef.Application.Contracts.Common;

public interface IDailyIntakeHelper
{
    short DeriveDailyCaloriesIntake(short age, string gender, float height, float weight, string activityLevel, string goal);
}
