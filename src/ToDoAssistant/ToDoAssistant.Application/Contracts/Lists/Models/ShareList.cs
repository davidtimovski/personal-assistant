using Core.Application.Contracts;
using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ShareList
{
    public required int UserId { get; init; }
    public required int ListId { get; init; }
    public required List<ShareUserAndPermission> NewShares { get; init; }
    public required List<ShareUserAndPermission> EditedShares { get; init; }
    public required List<ShareUserAndPermission> RemovedShares { get; init; }
}

public class ShareListValidator : AbstractValidator<ShareList>
{
    public ShareListValidator(IListService listService, IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var ownsOrSharesResult = listService.UserOwnsOrSharesAsAdmin(dto.ListId, userId);
                if (ownsOrSharesResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return ownsOrSharesResult.Data;
            }).WithMessage("Unauthorized");

        RuleForEach(dto => dto.NewShares).SetValidator(new ShareUserAndPermissionValidator(userService));
        RuleForEach(dto => dto.EditedShares).SetValidator(new ShareUserAndPermissionValidator(userService));
        RuleForEach(dto => dto.RemovedShares).SetValidator(new ShareUserAndPermissionValidator(userService));

        RuleFor(dto => dto).Must(dto =>
            {
                var userIds = dto.NewShares.Select(x => x.UserId)
                    .Concat(dto.EditedShares.Select(x => x.UserId))
                    .Concat(dto.RemovedShares.Select(x => x.UserId)).ToList();

                return userIds.Count == userIds.Distinct().Count();
            }).WithMessage("AnErrorOccurred")
            .Must(dto =>
            {
                bool sharedWithCurrentUser = dto.NewShares.Any(x => x.UserId == dto.UserId)
                                             || dto.EditedShares.Any(x => x.UserId == dto.UserId)
                                             || dto.RemovedShares.Any(x => x.UserId == dto.UserId);

                return !sharedWithCurrentUser;
            }).WithMessage("AnErrorOccurred");
    }
}
