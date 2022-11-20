using Application.Contracts;
using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ShareUserAndPermission
{
    public int UserId { get; set; }
    public bool IsAdmin { get; set; }
}

public class ShareUserAndPermissionValidator : AbstractValidator<ShareUserAndPermission>
{
    private const string prefix = nameof(ShareUserAndPermission);

    public ShareUserAndPermissionValidator(IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must(userId => userService.Exists(userId)).WithMessage($"{prefix}.UserDoesNotExist");
    }
}