﻿using System.Linq;
using FluentValidation;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class CopyList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class CopyListValidator : AbstractValidator<CopyList>, IValidator<CopyList>
    {
        private const string prefix = nameof(CopyListValidator);
        private static readonly string[] ListIcons = new string[] { "list", "shopping", "home", "birthday", "cheers", "vacation", "plane", "car", "pickup-truck", "world", "camping", "motorcycle", "bicycle", "ski", "snowboard", "work", "baby", "dog", "cat", "fish", "camera", "medicine", "file", "book", "mountain" };

        public CopyListValidator(IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .Must((dto, userId) => listService.UserOwnsOrShares(dto.Id, userId)).WithMessage("Unauthorized")
                .Must((dto, userId) => !listService.Exists(dto.Name, userId)).WithMessage("AlreadyExists")
                .Must(userId => listService.Count(userId) < 50).WithMessage("Lists.ListLimitReached");

            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
                .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

            RuleFor(dto => dto.Icon)
                .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
                .Must(icon => ListIcons.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
        }
    }
}
