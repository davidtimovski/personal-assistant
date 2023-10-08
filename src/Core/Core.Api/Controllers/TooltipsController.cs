using Api.Common;
using Core.Api.Models.Tooltips.Requests;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TooltipsController : BaseController
{
    private readonly ITooltipService _tooltipService;

    public TooltipsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        ITooltipService tooltipService) : base(userIdLookup, usersRepository)
    {
        _tooltipService = tooltipService;
    }

    [HttpGet("application/{application}")]
    public IActionResult GetAll(string application)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips/application/{application}",
            $"{nameof(TooltipsController)}.{nameof(GetAll)}",
            UserId
        );

        var tooltipDtos = _tooltipService.GetAll(application, UserId, tr);

        tr.Finish();

        return Ok(tooltipDtos);
    }

    [HttpGet("key/{key}/{application}")]
    public IActionResult GetByKey(string key, string application)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips/key/{key}/{application}",
            $"{nameof(TooltipsController)}.{nameof(GetByKey)}",
            UserId
        );

        var tooltipDto = _tooltipService.GetByKey(UserId, key, application, tr);

        tr.Finish();

        return Ok(tooltipDto);
    }

    [HttpPut]
    public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissedRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips",
            $"{nameof(TooltipsController)}.{nameof(ToggleDismissed)}",
            UserId
        );

        await _tooltipService.ToggleDismissedAsync(UserId, request.Key, request.Application, request.IsDismissed, tr);

        tr.Finish();

        return NoContent();
    }
}
