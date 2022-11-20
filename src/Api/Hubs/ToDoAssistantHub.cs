using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using ToDoAssistant.Application.Contracts.Lists;

namespace Api.Hubs;

[Authorize]
[EnableCors("AllowToDoAssistant")]
public class ToDoAssistantHub : Hub
{
    private readonly IUserIdLookup _userIdLookup;
    private readonly IUsersRepository _usersRepository;
    private readonly IListsRepository _listsRepository;

    public ToDoAssistantHub(
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
            string auth0Id = Context.User.Identity.Name;

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
