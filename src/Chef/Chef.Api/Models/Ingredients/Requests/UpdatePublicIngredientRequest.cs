using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Ingredients.Requests;

public record UpdatePublicIngredientRequest([Required] int Id, [Required] int? TaskId);
