using FluentValidation;

namespace Chef.Application.Contracts.Ingredients.Models;

public class UpdatePublicIngredient
{
    public required int UserId { get; init; }
    public required int Id { get; init; }
    public required int? TaskId { get; init; }
}

public class UpdatePublicIngredientValidator : AbstractValidator<UpdatePublicIngredient>
{
    public UpdatePublicIngredientValidator(IIngredientService ingredientService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => ingredientService.Exists(dto.Id, 1)).WithMessage("Unauthorized");

        // TODO
        //RuleFor(dto => dto.TaskId)
        //    .Must((dto, taskId) => !taskId.HasValue || taskService.Exists(taskId.Value, dto.UserId)).WithMessage("IsLinkedToNonExistentTask");
    }
}
