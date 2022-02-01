﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Infrastructure.Identity;

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
        public IActionResult GetAll(string application)
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

            var tooltipDtos = _tooltipService.GetAll(application, userId);

            return Ok(tooltipDtos);
        }

        [HttpGet("key/{key}/{application}")]
        public IActionResult GetByKey(string key, string application)
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

            var tooltipDto = _tooltipService.GetByKey(userId, key, application);

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