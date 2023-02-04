﻿using Core.Application.Contracts;
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
        UserDto user = null;

        switch (application)
        {
            case "To Do Assistant":
                user = _userService.Get<ToDoAssistantUser>(UserId);
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

    [HttpGet("cooking-preferences")]
    public IActionResult GetCookingPreferences()
    {
        CookingAssistantPreferences preferences = _userService.GetCookingAssistantPreferences(UserId);

        return Ok(preferences);
    }

    [HttpPut("to-do-notifications-enabled")]
    public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUserPreferences dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateToDoNotificationsEnabledAsync(UserId, dto.ToDoNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("cooking-notifications-enabled")]
    public async Task<IActionResult> UpdateCookingNotificationsEnabled([FromBody] UpdateUserPreferences dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateCookingNotificationsEnabledAsync(UserId, dto.CookingNotificationsEnabled);

        return NoContent();
    }

    [HttpPut("imperial-system")]
    public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUserPreferences dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _userService.UpdateImperialSystemAsync(UserId, dto.ImperialSystem);

        return NoContent();
    }
}