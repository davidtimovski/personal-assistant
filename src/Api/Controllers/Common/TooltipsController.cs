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
public class TooltipsController : BaseController
{
    private readonly ITooltipService _tooltipService;

    public TooltipsController(ITooltipService tooltipService)
    {
        _tooltipService = tooltipService;
    }

    [HttpGet("application/{application}")]
    public IActionResult GetAll(string application)
    {
        var tooltipDtos = _tooltipService.GetAll(application, CurrentUserId);

        return Ok(tooltipDtos);
    }

    [HttpGet("key/{key}/{application}")]
    public IActionResult GetByKey(string key, string application)
    {
        var tooltipDto = _tooltipService.GetByKey(CurrentUserId, key, application);

        return Ok(tooltipDto);
    }

    [HttpPut]
    public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissed dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        await _tooltipService.ToggleDismissedAsync(CurrentUserId, dto.Key, dto.Application, dto.IsDismissed);

        return NoContent();
    }
}
