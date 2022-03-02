using Application.Contracts.ToDoAssistant.Tasks;
using FluentValidation;

namespace Application.Contracts.CookingAssistant.Ingredients.Models;

public class UpdatePublicIngredient
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? TaskId { get; set; }
}

public class UpdatePublicIngredientValidator : AbstractValidator<UpdatePublicIngredient>
{
    public UpdatePublicIngredientValidator(IIngredientService ingredientService, ITaskService taskService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => ingredientService.Exists(dto.Id, 1)).WithMessage("Unauthorized");

        RuleFor(dto => dto.TaskId)
            .Must((dto, taskId) => !taskId.HasValue || taskService.Exists(taskId.Value, dto.UserId)).WithMessage("IsLinkedToNonExistentTask");
    }
}