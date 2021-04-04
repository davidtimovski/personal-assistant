using System.Linq;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Mappings
{
    public class ToDoAssistantProfile : Profile
    {
        public ToDoAssistantProfile()
        {
            CreateMap<CreateList, ToDoList>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.NotificationsEnabled, src => src.Ignore())
                .ForMember(x => x.IsArchived, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Tasks, src => src.Ignore())
                .ForMember(x => x.Shares, src => src.Ignore());
            CreateMap<UpdateList, ToDoList>()
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.IsArchived, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Tasks, src => src.Ignore())
                .ForMember(x => x.Shares, src => src.Ignore());
            CreateMap<UpdateSharedList, ToDoList>()
                .ForMember(x => x.Name, src => src.Ignore())
                .ForMember(x => x.Icon, src => src.Ignore())
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.IsOneTimeToggleDefault, src => src.Ignore())
                .ForMember(x => x.IsArchived, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Tasks, src => src.Ignore())
                .ForMember(x => x.Shares, src => src.Ignore());
            CreateMap<CopyList, ToDoList>()
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.NotificationsEnabled, src => src.Ignore())
                .ForMember(x => x.IsOneTimeToggleDefault, src => src.Ignore())
                .ForMember(x => x.IsArchived, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Tasks, src => src.Ignore())
                .ForMember(x => x.Shares, src => src.Ignore());

            CreateMap<CreateTask, ToDoTask>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.IsCompleted, src => src.Ignore())
                .ForMember(t => t.PrivateToUserId, opt => opt.MapFrom<PrivateToUserIdCreateResolver>())
                .ForMember(x => x.AssignedToUserId, src => src.Ignore())
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.List, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Recipes, src => src.Ignore());
            CreateMap<BulkCreate, ToDoTask>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.Name, src => src.Ignore())
                .ForMember(x => x.IsCompleted, src => src.Ignore())
                .ForMember(x => x.IsOneTime, src => src.Ignore())
                .ForMember(t => t.PrivateToUserId, opt => opt.Ignore())
                .ForMember(x => x.AssignedToUserId, src => src.Ignore())
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.List, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Recipes, src => src.Ignore());
            CreateMap<UpdateTask, ToDoTask>()
                .ForMember(x => x.IsCompleted, src => src.Ignore())
                .ForMember(t => t.PrivateToUserId, opt => opt.MapFrom<PrivateToUserIdUpdateResolver>())
                .ForMember(x => x.Order, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.List, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.Recipes, src => src.Ignore());

            CreateMap<CreateOrUpdateNotification, Notification>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.IsSeen, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore());

            CreateMap<User, ToDoAssistantPreferences>()
                .ForMember(x => x.NotificationsEnabled, opt => opt.MapFrom(src => src.ToDoNotificationsEnabled));
        }
    }

    public class ListSharingStateResolver : IValueResolver<ToDoList, object, ListSharingState>
    {
        public ListSharingState Resolve(ToDoList source, object dest, ListSharingState destMember, ResolutionContext context)
        {
            var userId = (int)context.Items["UserId"];

            if (source.Shares.Any())
            {
                bool someRequestsAccepted = source.Shares.Any(x => x.IsAccepted == true);
                if (someRequestsAccepted)
                {
                    if (source.UserId == userId)
                    {
                        return ListSharingState.Owner;
                    }

                    ListShare userShare = source.Shares.Single(x => x.UserId == userId);
                    return userShare.IsAdmin ? ListSharingState.Admin : ListSharingState.Member;
                }

                bool someRequestsPending = source.Shares.Any(x => !x.IsAccepted.HasValue);
                if (someRequestsPending)
                {
                    return ListSharingState.PendingShare;
                }
            }

            return ListSharingState.NotShared;
        }
    }

    public class ListOrderResolver : IValueResolver<ToDoList, object, short?>
    {
        public short? Resolve(ToDoList source, object dest, short? destMember, ResolutionContext context)
        {
            var userId = (int)context.Items["UserId"];

            if (source.Shares.Any())
            {
                var share = source.Shares.First();
                if (share.UserId == userId)
                {
                    return share.Order;
                }
            }

            return source.Order;
        }
    }

    public class ListNotificationsEnabledResolver : IValueResolver<ToDoList, EditListDto, bool>
    {
        public bool Resolve(ToDoList source, EditListDto dest, bool destMember, ResolutionContext context)
        {
            var userId = (int)context.Items["UserId"];

            if (source.Shares.Any())
            {
                var share = source.Shares.First();
                if (share.UserId == userId)
                {
                    return share.NotificationsEnabled;
                }
            }

            return source.NotificationsEnabled;
        }
    }

    public class IsArchivedResolver : IValueResolver<ToDoList, object, bool>
    {
        public bool Resolve(ToDoList source, object dest, bool destMember, ResolutionContext context)
        {
            var userId = (int)context.Items["UserId"];

            if (source.Shares.Any())
            {
                var share = source.Shares.First();
                if (share.UserId == userId)
                {
                    return share.IsArchived;
                }
            }

            return source.IsArchived;
        }
    }

    public class PrivateToUserIdCreateResolver : IValueResolver<CreateTask, ToDoTask, int?>
    {
        public int? Resolve(CreateTask source, ToDoTask dest, int? destMember, ResolutionContext context)
        {
            return source.IsPrivate.HasValue && source.IsPrivate.Value ? (int?)source.UserId : null;
        }
    }

    public class PrivateToUserIdUpdateResolver : IValueResolver<UpdateTask, ToDoTask, int?>
    {
        public int? Resolve(UpdateTask source, ToDoTask dest, int? destMember, ResolutionContext context)
        {
            return source.IsPrivate.HasValue && source.IsPrivate.Value ? (int?)source.UserId : null;
        }
    }

    public class TaskAssignedUserResolver : IValueResolver<ToDoTask, object, AssignedUser>
    {
        public AssignedUser Resolve(ToDoTask source, object dest, AssignedUser destMember, ResolutionContext context)
        {
            if (source.User != null)
            {
                return new AssignedUser
                {
                    Id = source.User.Id,
                    ImageUri = source.User.ImageUri
                };
            }

            return null;
        }
    }

    public class IsPrivateResolver : IValueResolver<ToDoTask, TaskForUpdate, bool>
    {
        public bool Resolve(ToDoTask source, TaskForUpdate dest, bool destMember, ResolutionContext context)
        {
            var userId = (int)context.Items["UserId"];
            return source.PrivateToUserId.HasValue && source.PrivateToUserId == userId ? true : false;
        }
    }

    public class ListWithSharesUserShareResolver : IValueResolver<ToDoList, ListWithShares, ListShareDto>
    {
        private readonly ICdnService _cdnService;

        public ListWithSharesUserShareResolver(ICdnService cdnService)
        {
            _cdnService = cdnService;
        }

        public ListShareDto Resolve(ToDoList source, ListWithShares dest, ListShareDto destMember, ResolutionContext context)
        {
            var shareDto = new ListShareDto();
            var userId = (int)context.Items["UserId"];

            var userShare = source.Shares.FirstOrDefault(x => x.UserId == userId);
            if (userShare != null)
            {
                shareDto.Email = userShare.User.Email;
                shareDto.ImageUri = _cdnService.ImageUriToThumbnail(userShare.User.ImageUri);
                shareDto.IsAdmin = userShare.IsAdmin;
                return shareDto;
            }

            return null;
        }
    }
}
