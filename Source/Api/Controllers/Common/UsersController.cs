using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Common
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [EnableCors("AllowAllApps")]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICdnService _cdnService;

        public UsersController(
            IUserService userService,
            ICdnService cdnService)
        {
            _userService = userService;
            _cdnService = cdnService;
        }

        [HttpGet("language")]
        public async Task<IActionResult> GetLanguage()
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

            string language = await _userService.GetLanguageAsync(userId);

            return Ok(language);
        }

        [HttpGet("profile-image-uri")]
        public async Task<IActionResult> GetProfileImageUri()
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

            string imageUri = await _userService.GetImageUriAsync(userId);

            return Ok(imageUri);
        }

        [HttpGet("to-do-preferences")]
        public async Task<IActionResult> GetToDoPreferences()
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

            ToDoAssistantPreferences preferences = await _userService.GetToDoAssistantPreferencesAsync(userId);

            return Ok(preferences);
        }

        [HttpGet("cooking-preferences")]
        public async Task<IActionResult> GetCookingPreferences()
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

            CookingAssistantPreferences preferences = await _userService.GetCookingAssistantPreferencesAsync(userId);

            return Ok(preferences);
        }

        [HttpPut("to-do-notifications-enabled")]
        public async Task<IActionResult> UpdateToDoNotificationsEnabled([FromBody] UpdateUser dto)
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

            await _userService.UpdateToDoNotificationsEnabledAsync(userId, dto.ToDoNotificationsEnabled);

            return NoContent();
        }

        [HttpPut("cooking-notifications-enabled")]
        public async Task<IActionResult> UpdateCookingNotificationsEnabled([FromBody] UpdateUser dto)
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

            await _userService.UpdateCookingNotificationsEnabledAsync(userId, dto.CookingNotificationsEnabled);

            return NoContent();
        }

        [HttpPut("imperial-system")]
        public async Task<IActionResult> UpdateImperialSystem([FromBody] UpdateUser dto)
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

            await _userService.UpdateImperialSystemAsync(userId, dto.ImperialSystem);

            return NoContent();
        }
    }
}