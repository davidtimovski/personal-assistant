﻿using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ToDoAssistant.Application.Contracts.Lists;

namespace ToDoAssistant.Api.Hubs;

[Authorize]
public class ListActionsHub : Hub
{
    private readonly IUserIdLookup _userIdLookup;
    private readonly IUsersRepository _usersRepository;
    private readonly IListsRepository _listsRepository;

    public ListActionsHub(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IListsRepository listsRepository)
    {
        _userIdLookup = userIdLookup;
        _usersRepository = usersRepository;
        _listsRepository = listsRepository;
    }

    private int UserId
    {
        get
        {
            var auth0Id = Context.User?.Identity?.Name;
            if (auth0Id is null)
            {
                throw new InvalidOperationException($"{nameof(UserId)} invoked for non-authenticated user");
            }

            if (_userIdLookup.Contains(auth0Id))
            {
                return _userIdLookup.Get(auth0Id);
            }
            else
            {
                var userId = _usersRepository.GetId(auth0Id);
                if (!userId.HasValue)
                {
                    throw new Exception($"The user with auth0_id '{auth0Id}' does not have a mapping");
                }

                _userIdLookup.Set(auth0Id, userId.Value);
                return userId.Value;
            }
        }
    }

    public async Task JoinGroups()
    {
        var listIds = _listsRepository.GetNonArchivedSharedListIds(UserId);

        foreach (var listId in listIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, listId.ToString());
        }
    }
}
