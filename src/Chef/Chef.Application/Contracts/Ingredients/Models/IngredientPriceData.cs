using AutoMapper;
using Chef.Application.Entities;
using Core.Application.Mappings;

namespace Chef.Application.Contracts.Ingredients.Models;

public class IngredientPriceData : IMapFrom<Ingredient>
{
    public bool IsSet { get; init; }
    public short ProductSize { get; init; }
    public bool ProductSizeIsOneUnit { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientPriceData>()
            .ForMember(x => x.IsSet, opt => opt.MapFrom(src => src.Price.HasValue));
    }
}
