using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Common
{
    [Authorize]
    [EnableCors("AllowAllApps")]
    [Route("api/[controller]")]
    public class TooltipsController : Controller
    {
        private readonly ITooltipService _tooltipService;

        public TooltipsController(ITooltipService tooltipService)
        {
            _tooltipService = tooltipService;
        }

        [HttpGet("application/{application}")]
        public async Task<IActionResult> GetAll(string application)
        {
            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            var tooltipDtos = await _tooltipService.GetAllAsync(application, userId);

            return Ok(tooltipDtos);
        }

        [HttpGet("key/{key}/{application}")]
        public async Task<IActionResult> GetByKey(string key, string application)
        {
            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            var tooltipDto = await _tooltipService.GetByKeyAsync(userId, key, application);

            return Ok(tooltipDto);
        }

        [HttpPut]
        public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissed dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _tooltipService.ToggleDismissedAsync(userId, dto.Key, dto.Application, dto.IsDismissed);

            return NoContent();
        }
    }
}