using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.ToDoAssistant.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

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
            string auth0Id = Context.User.FindFirst("sub").Value;

            if (_userIdLookup.Contains(auth0Id))
            {
                return _userIdLookup.Get(auth0Id);
            }
            else
            {
                var userId = _usersRepository.GetId(auth0Id);
                _userIdLookup.Set(auth0Id, userId);
                return userId;
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
