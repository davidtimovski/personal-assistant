namespace Chef.Api.Models.DietaryProfiles.Requests;

public record GetRecommendedDailyIntakeRequest(DateTime Birthday, string? Gender, short? HeightCm, short? HeightFeet, short? HeightInches, short? WeightKg, short? WeightLbs, string? ActivityLevel, string? Goal);
