using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IUserService userService) : base(userIdLookup, usersRepository)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Get(string application)
    {
        UserDto? user = null;

        switch (application)
        {
            case "To Do Assistant":
                user = _userService.Get<ToDoAssistantUser>(UserId);
                break;
            case "Chef":
                user = _userService.Get<ChefUser>(UserId);
                break;
            case "Accountant":
                user = _userService.Get<AccountantUser>(UserId);
                break;
            case "Weatherman":
                user = _userService.Get<WeathermanUser>(UserId);
                break;
            case "Trainer":
                user = _userService.Get<TrainerUser>(UserId);
                break;
        }

        return Ok(user);
    }

    [HttpGet("chef-preferences")]
    public IActionResult GetChefPreferences()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/chef-preferences",
            $"{nameof(UsersController)}.{nameof(GetChefPreferences)}",
            UserId
        );

        ChefPreferences preferences = _userService.GetChefPreferences(UserId, tr);

        tr.Finish();

        return Ok(preferences);
    }

    [HttpPut("to-do-notifications-enabled")]
    public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUserPreferences dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/to-do-notifications-enabled",
            $"{nameof(UsersController)}.{nameof(UpdateToDoNotificationsEnabled)}",
            UserId
        );

        await _userService.UpdateToDoNotificationsEnabledAsync(UserId, dto.ToDoNotificationsEnabled, tr);

        tr.Finish();

        return NoContent();
    }

    [HttpPut("chef-notifications-enabled")]
    public async Task<IActionResult> UpdateChefNotificationsEnabled([FromBody] UpdateUserPreferences dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/chef-notifications-enabled",
            $"{nameof(UsersController)}.{nameof(UpdateChefNotificationsEnabled)}",
            UserId
        );

        await _userService.UpdateChefNotificationsEnabledAsync(UserId, dto.ChefNotificationsEnabled, tr);

        tr.Finish();

        return NoContent();
    }

    [HttpPut("imperial-system")]
    public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUserPreferences dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
           $"{Request.Method} api/users/imperial-system",
            $"{nameof(UsersController)}.{nameof(UpdateImperialSystem)}",
            UserId
        );

        await _userService.UpdateImperialSystemAsync(UserId, dto.ImperialSystem, tr);

        tr.Finish();

        return NoContent();
    }
}
