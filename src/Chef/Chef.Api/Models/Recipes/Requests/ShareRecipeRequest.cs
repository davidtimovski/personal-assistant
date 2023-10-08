using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record ShareRecipeRequest([Required] int RecipeId, [Required] List<int> NewShares, [Required] List<int> RemovedShares);
