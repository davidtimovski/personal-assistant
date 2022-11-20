using Application.Contracts;
using Application.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

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
        var tooltipDtos = _tooltipService.GetAll(application, UserId);

        return Ok(tooltipDtos);
    }

    [HttpGet("key/{key}/{application}")]
    public IActionResult GetByKey(string key, string application)
    {
        var tooltipDto = _tooltipService.GetByKey(UserId, key, application);

        return Ok(tooltipDto);
    }

    [HttpPut]
    public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissed dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _tooltipService.ToggleDismissedAsync(UserId, dto.Key, dto.Application, dto.IsDismissed);

        return NoContent();
    }
}
