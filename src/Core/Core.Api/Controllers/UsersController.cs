using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

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
        switch (application)
        {
            case "To Do Assistant":
                var todoUserResult = _userService.Get<ToDoAssistantUser>(UserId);
                if (todoUserResult.Failed)
                {
                    return StatusCode(500);
                }
                return Ok(todoUserResult.Data);
            case "Chef":
                var chefUserResult = _userService.Get<ChefUser>(UserId);
                if (chefUserResult.Failed)
                {
                    return StatusCode(500);
                }
                return Ok(chefUserResult.Data);
            case "Accountant":
                var accountantUserResult = _userService.Get<AccountantUser>(UserId);
                if (accountantUserResult.Failed)
                {
                    return StatusCode(500);
                }
                return Ok(accountantUserResult.Data);
            case "Weatherman":
                var weathermanUserResult = _userService.Get<WeathermanUser>(UserId);
                if (weathermanUserResult.Failed)
                {
                    return StatusCode(500);
                }
                return Ok(weathermanUserResult.Data);
            case "Trainer":
                var trainerUserResult = _userService.Get<TrainerUser>(UserId);
                if (trainerUserResult.Failed)
                {
                    return StatusCode(500);
                }
                return Ok(trainerUserResult.Data);
            default:
                return BadRequest();
        }
    }

    [HttpGet("chef-preferences")]
    public IActionResult GetChefPreferences()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/chef-preferences",
            $"{nameof(UsersController)}.{nameof(GetChefPreferences)}",
            UserId
        );

        try
        {
            var result = _userService.GetChefPreferences(UserId, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("to-do-notifications-enabled")]
    public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUserPreferences dto, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/to-do-notifications-enabled",
            $"{nameof(UsersController)}.{nameof(UpdateToDoNotificationsEnabled)}",
            UserId
        );

        try
        {
            if (dto is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _userService.UpdateToDoNotificationsEnabledAsync(UserId, dto.ToDoNotificationsEnabled, tr, cancellationToken);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("chef-notifications-enabled")]
    public async Task<IActionResult> UpdateChefNotificationsEnabled([FromBody] UpdateUserPreferences dto, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/users/chef-notifications-enabled",
            $"{nameof(UsersController)}.{nameof(UpdateChefNotificationsEnabled)}",
            UserId
        );

        try
        {
            if (dto is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _userService.UpdateChefNotificationsEnabledAsync(UserId, dto.ChefNotificationsEnabled, tr, cancellationToken);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("imperial-system")]
    public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUserPreferences dto, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
           $"{Request.Method} api/users/imperial-system",
            $"{nameof(UsersController)}.{nameof(UpdateImperialSystem)}",
            UserId
        );

        try
        {
            if (dto is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _userService.UpdateImperialSystemAsync(UserId, dto.ImperialSystem, tr, cancellationToken);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }
}
