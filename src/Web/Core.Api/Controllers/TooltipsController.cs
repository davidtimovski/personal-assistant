using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
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
            "GET api/tooltips/application/{application}",
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
            "GET api/tooltips/key/{key}/{application}",
            $"{nameof(TooltipsController)}.{nameof(GetByKey)}",
            UserId
        );

        var tooltipDto = _tooltipService.GetByKey(UserId, key, application, tr);

        tr.Finish();

        return Ok(tooltipDto);
    }

    [HttpPut]
    public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissed dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            "PUT api/tooltips",
            $"{nameof(TooltipsController)}.{nameof(ToggleDismissed)}",
            UserId
        );

        await _tooltipService.ToggleDismissedAsync(UserId, dto.Key, dto.Application, dto.IsDismissed, tr);

        tr.Finish();

        return NoContent();
    }
}
