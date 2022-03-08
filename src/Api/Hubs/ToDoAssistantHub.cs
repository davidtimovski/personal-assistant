using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Lists;
using Infrastructure.Identity;
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

    public async Task JoinGroups()
    {
        int userId = IdentityHelper.GetUserId(Context.User);
        var listIds = _listsRepository.GetNonArchivedSharedListIds(userId);

        foreach (var listId in listIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, listId.ToString());
        }
    }
}
