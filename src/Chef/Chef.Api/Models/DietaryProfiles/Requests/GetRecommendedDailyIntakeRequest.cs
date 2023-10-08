using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.DietaryProfiles.Requests;

public record GetRecommendedDailyIntakeRequest([Required] DateTime Birthday, [Required] string? Gender, [Required] short? HeightCm, [Required] short? HeightFeet, [Required] short? HeightInches, [Required] short? WeightKg, [Required] short? WeightLbs, [Required] string? ActivityLevel, [Required] string? Goal);
