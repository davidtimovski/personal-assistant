using System.ComponentModel.DataAnnotations;
using Chef.Application.Contracts.Recipes.Models;

namespace Chef.Api.Models.Recipes.Requests;

public record ImportRecipeRequest([Required] int Id, [Required] List<IngredientReplacement> IngredientReplacements, [Required] bool CheckIfReviewRequired);
