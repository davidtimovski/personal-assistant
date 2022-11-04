using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.Weatherman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Common;

[Authorize]
[EnableCors("AllowAllApps")]
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
        UserDto user = null;

        switch (application)
        {
            case "Weatherman":
                user = _userService.Get<WeathermanUser>(UserId);
                break;
        }

        return Ok(user);
    }

    [HttpGet("to-do-preferences")]
    public IActionResult GetToDoPreferences()
    {
        ToDoAssistantPreferences preferences = _userService.GetToDoAssistantPreferences(UserId);

        return Ok(preferences);
    }

    [HttpGet("cooking-preferences")]
    public IActionResult GetCookingPreferences()
    {
        CookingAssistantPreferences preferences = _userService.GetCookingAssistantPreferences(UserId);

        return Ok(preferences);
    }

    [HttpPut("to-do-notifications-enabled")]
    public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateToDoNotificationsEnabledAsync(UserId, dto.ToDoNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("cooking-notifications-enabled")]
    public async Task<IActionResult> UpdateCookingNotificationsEnabled([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateCookingNotificationsEnabledAsync(UserId, dto.CookingNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("imperial-system")]
    public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateImperialSystemAsync(UserId, dto.ImperialSystem);

        return NoContent();
    }
}
