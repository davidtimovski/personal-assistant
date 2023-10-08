using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record SetShareIsAcceptedRequest([Required] int RecipeId, [Required] bool IsAccepted);
