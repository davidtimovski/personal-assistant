using Application.Contracts;
using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class ShareList
{
    public int UserId { get; set; }
    public int ListId { get; set; }
    public List<ShareUserAndPermission> NewShares { get; set; }
    public List<ShareUserAndPermission> EditedShares { get; set; }
    public List<ShareUserAndPermission> RemovedShares { get; set; }
}

public class ShareListValidator : AbstractValidator<ShareList>
{
    public ShareListValidator(IListService listService, IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => listService.UserOwnsOrSharesAsAdmin(dto.ListId, userId)).WithMessage("Unauthorized");

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