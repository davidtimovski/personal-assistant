namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientPriceData(bool IsSet, short ProductSize, bool ProductSizeIsOneUnit, decimal? Price, string? Currency);
