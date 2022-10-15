using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
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

    [HttpGet("language")]
    public IActionResult GetLanguage()
    {
        string language = _userService.GetLanguage(CurrentUserId);

        return Ok(language);
    }

    [HttpGet("profile-image-uri")]
    public IActionResult GetProfileImageUri()
    {
        string imageUri = _userService.GetImageUri(CurrentUserId);

        return Ok(imageUri);
    }

    [HttpGet("to-do-preferences")]
    public IActionResult GetToDoPreferences()
    {
        ToDoAssistantPreferences preferences = _userService.GetToDoAssistantPreferences(CurrentUserId);

        return Ok(preferences);
    }

    [HttpGet("cooking-preferences")]
    public IActionResult GetCookingPreferences()
    {
        CookingAssistantPreferences preferences = _userService.GetCookingAssistantPreferences(CurrentUserId);

        return Ok(preferences);
    }

    [HttpPut("to-do-notifications-enabled")]
    public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateToDoNotificationsEnabledAsync(CurrentUserId, dto.ToDoNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("cooking-notifications-enabled")]
    public async Task<IActionResult> UpdateCookingNotificationsEnabled([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateCookingNotificationsEnabledAsync(CurrentUserId, dto.CookingNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("imperial-system")]
    public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUser dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateImperialSystemAsync(CurrentUserId, dto.ImperialSystem);

        return NoContent();
    }
}
