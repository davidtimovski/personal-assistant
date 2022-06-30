using System.Security.Claims;
using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

[Authorize]
[EnableCors("AllowToDoAssistant")]
public class ToDoAssistantHub : Hub
{
    private readonly IListsRepository _listsRepository;

    public ToDoAssistantHub(IListsRepository listsRepository)
    {
        _listsRepository = listsRepository;
    }

    private int CurrentUserId
    {
        get
        {
            string id = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return int.Parse(id);
        }
    }

    public async Task JoinGroups()
    {
        var listIds = _listsRepository.GetNonArchivedSharedListIds(CurrentUserId);

        foreach (var listId in listIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, listId.ToString());
        }
    }
}
