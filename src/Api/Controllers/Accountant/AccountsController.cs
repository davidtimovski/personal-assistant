using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Accountant
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [EnableCors("AllowAccountant")]
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccount dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            try
            {
                dto.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            int id = await _accountService.CreateAsync(dto);

            return StatusCode(201, id);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAccount dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            try
            {
                dto.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _accountService.UpdateAsync(dto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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

            await _accountService.DeleteAsync(id, userId);

            return NoContent();
        }
    }
}