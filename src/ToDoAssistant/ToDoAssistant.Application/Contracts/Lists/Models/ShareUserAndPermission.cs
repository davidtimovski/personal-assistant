using Core.Application.Contracts;
using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ShareUserAndPermission
{
    public required int UserId { get; init; }
    public required bool IsAdmin { get; init; }
}

public class ShareUserAndPermissionValidator : AbstractValidator<ShareUserAndPermission>
{
    private const string prefix = nameof(ShareUserAndPermission);

    public ShareUserAndPermissionValidator(IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must(userId =>
            {
                var existsResult = userService.Exists(userId);
                if (existsResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return existsResult.Data;
            }).WithMessage($"{prefix}.UserDoesNotExist");
    }
}
