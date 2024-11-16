using AutoMapper;
using Core.Application.Contracts;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications.Models;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Mappings;

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
            .ForMember(x => x.IsHighPriority, src => src.Ignore())
            .ForMember(t => t.PrivateToUserId, opt => opt.MapFrom(src => src.IsPrivate.HasValue && src.IsPrivate.Value ? (int?)src.UserId : null))
            .ForMember(x => x.AssignedToUserId, src => src.Ignore())
            .ForMember(x => x.Order, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.List, src => src.Ignore())
            .ForMember(x => x.AssignedToUser, src => src.Ignore());
        CreateMap<BulkCreate, ToDoTask>()
            .ForMember(x => x.Id, src => src.Ignore())
            .ForMember(x => x.Name, src => src.Ignore())
            .ForMember(x => x.Url, src => src.Ignore())
            .ForMember(x => x.IsCompleted, src => src.Ignore())
            .ForMember(x => x.IsOneTime, src => src.Ignore())
            .ForMember(x => x.IsHighPriority, src => src.Ignore())
            .ForMember(t => t.PrivateToUserId, opt => opt.Ignore())
            .ForMember(x => x.AssignedToUserId, src => src.Ignore())
            .ForMember(x => x.Order, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.List, src => src.Ignore())
            .ForMember(x => x.AssignedToUser, src => src.Ignore());
        CreateMap<UpdateTask, ToDoTask>()
            .ForMember(x => x.IsCompleted, src => src.Ignore())
            .ForMember(t => t.PrivateToUserId, opt => opt.MapFrom(src => src.IsPrivate.HasValue && src.IsPrivate.Value ? (int?)src.UserId : null))
            .ForMember(x => x.Order, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore())
            .ForMember(x => x.List, src => src.Ignore())
            .ForMember(x => x.AssignedToUser, src => src.Ignore());

        CreateMap<CreateOrUpdateNotification, Notification>()
            .ForMember(x => x.Id, src => src.Ignore())
            .ForMember(x => x.IsSeen, src => src.Ignore())
            .ForMember(x => x.User, src => src.Ignore())
            .ForMember(x => x.CreatedDate, src => src.Ignore())
            .ForMember(x => x.ModifiedDate, src => src.Ignore());
    }
}

public class ListSharingStateResolver : IValueResolver<ToDoList, object, ListSharingState>
{
    public ListSharingState Resolve(ToDoList source, object destination, ListSharingState destinationMember, ResolutionContext context)
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
    public short? Resolve(ToDoList source, object destination, short? destinationMember, ResolutionContext context)
    {
        var userId = (int)context.Items["UserId"];

        var share = source.Shares.FirstOrDefault(x => x.UserId == userId);
        if (share is not null)
        {
            return share.Order;
        }

        return source.Order;
    }
}

public class ListNotificationsEnabledResolver : IValueResolver<ToDoList, EditListDto, bool>
{
    public bool Resolve(ToDoList source, EditListDto destination, bool destinationMember, ResolutionContext context)
    {
        var userId = (int)context.Items["UserId"];

        var share = source.Shares.FirstOrDefault(x => x.UserId == userId);
        if (share is not null)
        {
            return share.NotificationsEnabled;
        }

        return source.NotificationsEnabled;
    }
}

public class IsArchivedResolver : IValueResolver<ToDoList, object, bool>
{
    public bool Resolve(ToDoList source, object destination, bool destinationMember, ResolutionContext context)
    {
        var userId = (int)context.Items["UserId"];

        var share = source.Shares.FirstOrDefault(x => x.UserId == userId);
        if (share is not null)
        {
            return share.IsArchived;
        }

        return source.IsArchived;
    }
}

public class IsPrivateResolver : IValueResolver<ToDoTask, TaskForUpdate, bool>
{
    public bool Resolve(ToDoTask source, TaskForUpdate destination, bool destinationMember, ResolutionContext context)
    {
        var userId = (int)context.Items["UserId"];
        return source.PrivateToUserId.HasValue && source.PrivateToUserId == userId ? true : false;
    }
}

public class ListWithSharesUserShareResolver : IValueResolver<ToDoList, ListWithShares, ListShareDto?>
{
    private readonly ICdnService _cdnService;

    public ListWithSharesUserShareResolver(ICdnService cdnService)
    {
        _cdnService = cdnService;
    }

    public ListShareDto? Resolve(ToDoList source, ListWithShares destination, ListShareDto? destinationMember, ResolutionContext context)
    {
        var shareDto = new ListShareDto();
        var userId = (int)context.Items["UserId"];

        var userShare = source.Shares.FirstOrDefault(x => x.UserId == userId);
        if (userShare is not null)
        {
            shareDto.Email = userShare.User!.Email;
            shareDto.ImageUri = _cdnService.ImageUriToThumbnail(new Uri(userShare.User.ImageUri)).ToString();
            shareDto.IsAdmin = userShare.IsAdmin;
            return shareDto;
        }

        return null;
    }
}
