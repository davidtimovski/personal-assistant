using Application.Domain.CookingAssistant;
using AutoMapper;
using Core.Application.Mappings;

namespace CookingAssistant.Application.Contracts.Ingredients.Models;

public class IngredientPriceData : IMapFrom<Ingredient>
{
    public bool IsSet { get; set; }
    public short ProductSize { get; set; }
    public bool ProductSizeIsOneUnit { get; set; }
    public decimal? Price { get; set; }
    public string Currency { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Ingredient, IngredientPriceData>()
            .ForMember(x => x.IsSet, opt => opt.MapFrom(src => src.Price.HasValue));
    }
}
