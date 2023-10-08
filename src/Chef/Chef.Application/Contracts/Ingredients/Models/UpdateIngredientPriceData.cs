namespace Chef.Application.Contracts.Ingredients.Models;

public class UpdateIngredientPriceData
{
    public required bool IsSet { get; init; }
    public required short ProductSize { get; init; }
    public required bool ProductSizeIsOneUnit { get; init; }
    public required decimal? Price { get; init; }
    public required string? Currency { get; init; }
}
