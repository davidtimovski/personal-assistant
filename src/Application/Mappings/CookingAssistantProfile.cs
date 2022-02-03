using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using Application.Contracts.CookingAssistant.Recipes.Models;
using Domain.Entities.Common;
using Domain.Entities.CookingAssistant;
using Utility;

namespace Application.Mappings;

public class CookingAssistantProfile : Profile
{
    public CookingAssistantProfile()
    {
        CreateMap<CreateRecipe, Recipe>()
            .ForMember(x => x.Id, src => src.Ignore())
            .ForMember(x => x.LastOpenedDate, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.User, src => src.Ignore())
            .ForMember(x => x.RecipeIngredients, opt => opt.MapFrom(src => src.Ingredients))
            .ForMember(x => x.Shares, opt => opt.Ignore())
            .ForMember(x => x.IngredientsMissing, src => src.Ignore());
        CreateMap<UpdateRecipe, Recipe>()
            .ForMember(x => x.UserId, src => src.Ignore())
            .ForMember(x => x.LastOpenedDate, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.User, src => src.Ignore())
            .ForMember(x => x.RecipeIngredients, opt => opt.MapFrom(src => src.Ingredients))
            .ForMember(x => x.Shares, opt => opt.Ignore())
            .ForMember(x => x.IngredientsMissing, src => src.Ignore());
        CreateMap<UpdateRecipeIngredient, RecipeIngredient>()
            .ForMember(x => x.RecipeId, src => src.Ignore())
            .ForMember(x => x.IngredientId, opt => opt.MapFrom(src => src.Id.HasValue ? src.Id.Value : 0))
            .ForPath(x => x.Ingredient.TaskId, opt => opt.MapFrom(src => src.TaskId))
            .ForPath(x => x.Ingredient.Name, opt => opt.MapFrom(src => src.Name))
            .ForPath(x => x.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.Recipe, src => src.Ignore());

        CreateMap<UpdateIngredient, Ingredient>()
            .ForMember(x => x.ServingSize, opt => opt.MapFrom(src => src.NutritionData.ServingSize))
            .ForMember(x => x.ServingSizeIsOneUnit, opt => opt.MapFrom(src => src.NutritionData.ServingSizeIsOneUnit))
            .ForMember(x => x.Calories, opt => opt.MapFrom(src => src.NutritionData.Calories))
            .ForMember(x => x.Fat, opt => opt.MapFrom(src => src.NutritionData.Fat))
            .ForMember(x => x.SaturatedFat, opt => opt.MapFrom(src => src.NutritionData.SaturatedFat))
            .ForMember(x => x.Carbohydrate, opt => opt.MapFrom(src => src.NutritionData.Carbohydrate))
            .ForMember(x => x.Sugars, opt => opt.MapFrom(src => src.NutritionData.Sugars))
            .ForMember(x => x.AddedSugars, opt => opt.MapFrom(src => src.NutritionData.AddedSugars))
            .ForMember(x => x.Fiber, opt => opt.MapFrom(src => src.NutritionData.Fiber))
            .ForMember(x => x.Protein, opt => opt.MapFrom(src => src.NutritionData.Protein))
            .ForMember(x => x.Sodium, opt => opt.MapFrom(src => src.NutritionData.Sodium))
            .ForMember(x => x.Cholesterol, opt => opt.MapFrom(src => src.NutritionData.Cholesterol))
            .ForMember(x => x.VitaminA, opt => opt.MapFrom(src => src.NutritionData.VitaminA))
            .ForMember(x => x.VitaminC, opt => opt.MapFrom(src => src.NutritionData.VitaminC))
            .ForMember(x => x.VitaminD, opt => opt.MapFrom(src => src.NutritionData.VitaminD))
            .ForMember(x => x.Calcium, opt => opt.MapFrom(src => src.NutritionData.Calcium))
            .ForMember(x => x.Iron, opt => opt.MapFrom(src => src.NutritionData.Iron))
            .ForMember(x => x.Potassium, opt => opt.MapFrom(src => src.NutritionData.Potassium))
            .ForMember(x => x.Magnesium, opt => opt.MapFrom(src => src.NutritionData.Magnesium))
            .ForMember(x => x.ProductSize, opt => opt.MapFrom(src => src.PriceData.ProductSize))
            .ForMember(x => x.ProductSizeIsOneUnit, opt => opt.MapFrom(src => src.PriceData.ProductSizeIsOneUnit))
            .ForMember(x => x.Price, opt => opt.MapFrom(src => src.PriceData.Price))
            .ForMember(x => x.Currency, opt => opt.MapFrom(src => src.PriceData.Currency))
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.Recipes, src => src.Ignore())
            .ForMember(x => x.Task, src => src.Ignore())
            .ForMember(x => x.RecipeCount, src => src.Ignore());

        CreateMap<UpdateDietaryProfile, DietaryProfile>()
            .ForMember(x => x.Height, src => src.Ignore())
            .ForMember(x => x.Weight, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.User, src => src.Ignore());

        CreateMap<User, CookingAssistantPreferences>()
            .ForMember(x => x.NotificationsEnabled, opt => opt.MapFrom(src => src.CookingNotificationsEnabled));
    }
}

public class RecipeWithSharesUserShareResolver : IValueResolver<Recipe, RecipeWithShares, RecipeShareDto>
{
    private readonly ICdnService _cdnService;

    public RecipeWithSharesUserShareResolver(ICdnService cdnService)
    {
        _cdnService = cdnService;
    }

    public RecipeShareDto Resolve(Recipe source, RecipeWithShares dest, RecipeShareDto destMember, ResolutionContext context)
    {
        var shareDto = new RecipeShareDto();
        var userId = (int)context.Items["UserId"];

        var userShare = source.Shares.FirstOrDefault(x => x.UserId == userId);
        if (userShare != null)
        {
            shareDto.Email = userShare.User.Email;
            shareDto.ImageUri = _cdnService.ImageUriToThumbnail(userShare.User.ImageUri);
            return shareDto;
        }

        return null;
    }
}

public class LastOpenedDateResolver : IValueResolver<Recipe, object, DateTime>
{
    public DateTime Resolve(Recipe source, object dest, DateTime destMember, ResolutionContext context)
    {
        var userId = (int)context.Items["UserId"];

        if (source.Shares.Any())
        {
            var share = source.Shares.First();
            if (share.UserId == userId)
            {
                return share.LastOpenedDate;
            }
        }

        return source.LastOpenedDate;
    }
}

public class DurationResolver : IMemberValueResolver<object, object, TimeSpan?, string>
{
    public string Resolve(object source, object destination, TimeSpan? sourceMember, string destinationMember, ResolutionContext context)
    {
        return sourceMember.HasValue ? sourceMember.Value.ToString(@"hh\:mm") : string.Empty;
    }
}

public class RecipeNameResolver : IMemberValueResolver<object, object, List<Recipe>, List<string>>
{
    public List<string> Resolve(object source, object destination, List<Recipe> sourceMember, List<string> destinationMember, ResolutionContext context)
    {
        return sourceMember.Select(x => x.Name).ToList();
    }
}

public class RecipeIngredientHasNutritionDataResolver : IValueResolver<RecipeIngredient, object, bool>
{
    public bool Resolve(RecipeIngredient source, object dest, bool destMember, ResolutionContext context)
    {
        var hasNutritionData = source.Ingredient.Calories.HasValue
                               || source.Ingredient.Fat.HasValue
                               || source.Ingredient.SaturatedFat.HasValue
                               || source.Ingredient.Carbohydrate.HasValue
                               || source.Ingredient.Sugars.HasValue
                               || source.Ingredient.AddedSugars.HasValue
                               || source.Ingredient.Fiber.HasValue
                               || source.Ingredient.Protein.HasValue
                               || source.Ingredient.Sodium.HasValue
                               || source.Ingredient.Cholesterol.HasValue
                               || source.Ingredient.VitaminA.HasValue
                               || source.Ingredient.VitaminC.HasValue
                               || source.Ingredient.VitaminD.HasValue
                               || source.Ingredient.Calcium.HasValue
                               || source.Ingredient.Iron.HasValue
                               || source.Ingredient.Potassium.HasValue
                               || source.Ingredient.Magnesium.HasValue;

        return hasNutritionData;
    }
}

public class RecipeIngredientHasPriceDataResolver : IValueResolver<RecipeIngredient, object, bool>
{
    public bool Resolve(RecipeIngredient source, object dest, bool destMember, ResolutionContext context)
    {
        return source.Ingredient.Price.HasValue;
    }
}

public class IngredientHasNutritionDataResolver : IValueResolver<Ingredient, object, bool>
{
    public bool Resolve(Ingredient source, object dest, bool destMember, ResolutionContext context)
    {
        var hasNutritionData = source.Calories.HasValue
                               || source.Fat.HasValue
                               || source.SaturatedFat.HasValue
                               || source.Carbohydrate.HasValue
                               || source.Sugars.HasValue
                               || source.AddedSugars.HasValue
                               || source.Fiber.HasValue
                               || source.Protein.HasValue
                               || source.Sodium.HasValue
                               || source.Cholesterol.HasValue
                               || source.VitaminA.HasValue
                               || source.VitaminC.HasValue
                               || source.VitaminD.HasValue
                               || source.Calcium.HasValue
                               || source.Iron.HasValue
                               || source.Potassium.HasValue
                               || source.Magnesium.HasValue;

        return hasNutritionData;
    }
}

public class IngredientHasPriceDataResolver : IValueResolver<Ingredient, object, bool>
{
    public bool Resolve(Ingredient source, object dest, bool destMember, ResolutionContext context)
    {
        return source.Price.HasValue;
    }
}

public class NutritionDataResolver : IValueResolver<Ingredient, object, IngredientNutritionData>
{
    public IngredientNutritionData Resolve(Ingredient source, object dest, IngredientNutritionData destMember, ResolutionContext context)
    {
        var nutritionData = new IngredientNutritionData
        {
            ServingSize = source.ServingSize,
            ServingSizeIsOneUnit = source.ServingSizeIsOneUnit,
            Calories = source.Calories,
            Fat = source.Fat,
            SaturatedFat = source.SaturatedFat,
            Carbohydrate = source.Carbohydrate,
            Sugars = source.Sugars,
            AddedSugars = source.AddedSugars,
            Fiber = source.Fiber,
            Protein = source.Protein,
            Sodium = source.Sodium,
            Cholesterol = source.Cholesterol,
            VitaminA = source.VitaminA,
            VitaminC = source.VitaminC,
            VitaminD = source.VitaminD,
            Calcium = source.Calcium,
            Iron = source.Iron,
            Potassium = source.Potassium,
            Magnesium = source.Magnesium
        };

        nutritionData.IsSet = source.Calories.HasValue
                              || source.Fat.HasValue
                              || source.SaturatedFat.HasValue
                              || source.Carbohydrate.HasValue
                              || source.Sugars.HasValue
                              || source.AddedSugars.HasValue
                              || source.Fiber.HasValue
                              || source.Protein.HasValue
                              || source.Sodium.HasValue
                              || source.Cholesterol.HasValue
                              || source.VitaminA.HasValue
                              || source.VitaminC.HasValue
                              || source.VitaminD.HasValue
                              || source.Calcium.HasValue
                              || source.Iron.HasValue
                              || source.Potassium.HasValue
                              || source.Magnesium.HasValue;

        return nutritionData;
    }
}

public class PriceDataResolver : IValueResolver<Ingredient, object, IngredientPriceData>
{
    public IngredientPriceData Resolve(Ingredient source, object dest, IngredientPriceData destMember, ResolutionContext context)
    {
        var priceData = new IngredientPriceData
        {
            IsSet = source.Price.HasValue,
            ProductSize = source.ProductSize,
            ProductSizeIsOneUnit = source.ProductSizeIsOneUnit,
            Price = source.Price,
            Currency = source.Currency
        };

        return priceData;
    }
}

public class HeightCmResolver : IValueResolver<DietaryProfile, object, short?>
{
    public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
    {
        if (source.Height.HasValue && !source.User.ImperialSystem)
        {
            return (short)Math.Floor((double)source.Height);
        }

        return null;
    }
}

public class HeightFeetResolver : IValueResolver<DietaryProfile, object, short?>
{
    private readonly IConversion _conversion;

    public HeightFeetResolver(IConversion conversion)
    {
        _conversion = conversion;
    }

    public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
    {
        if (source.Height.HasValue && source.User.ImperialSystem)
        {
            var (feet, _) = _conversion.CentimetersToFeetAndInches(source.Height.Value);
            return (short?)feet;
        }

        return null;
    }
}

public class HeightInchesResolver : IValueResolver<DietaryProfile, object, short?>
{
    private readonly IConversion _conversion;

    public HeightInchesResolver(IConversion conversion)
    {
        _conversion = conversion;
    }

    public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
    {
        if (source.Height.HasValue && source.User.ImperialSystem)
        {
            var (_, inches) = _conversion.CentimetersToFeetAndInches(source.Height.Value);
            return (short?)inches;
        }

        return null;
    }
}

public class WeightKgResolver : IValueResolver<DietaryProfile, object, float?>
{
    public float? Resolve(DietaryProfile source, object dest, float? destMember, ResolutionContext context)
    {
        if (source.Weight.HasValue && !source.User.ImperialSystem)
        {
            return (float)Math.Round(source.Weight.Value, 1);
        }

        return null;
    }
}

public class WeightLbsResolver : IValueResolver<DietaryProfile, object, short?>
{
    private readonly IConversion _conversion;

    public WeightLbsResolver(IConversion conversion)
    {
        _conversion = conversion;
    }

    public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
    {
        if (source.Weight.HasValue && source.User.ImperialSystem)
        {
            var pounds = _conversion.KilosToPounds(source.Weight.Value);
            return (short?)pounds;
        }

        return null;
    }
}