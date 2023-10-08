using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientPriceData([Required] bool IsSet, [Required] short ProductSize, [Required] bool ProductSizeIsOneUnit, [Required] decimal? Price, [Required] string? Currency);
