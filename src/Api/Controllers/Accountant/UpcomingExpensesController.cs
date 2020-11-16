using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Accountant
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [EnableCors("AllowAccountant")]
    [Route("api/[controller]")]
    public class UpcomingExpensesController : Controller
    {
        private readonly IUpcomingExpenseService _upcomingExpenseService;

        public UpcomingExpensesController(IUpcomingExpenseService upcomingExpenseService)
        {
            _upcomingExpenseService = upcomingExpenseService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUpcomingExpense dto)
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

            int id = await _upcomingExpenseService.CreateAsync(dto);

            return StatusCode(201, id);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUpcomingExpense dto)
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

            await _upcomingExpenseService.UpdateAsync(dto);

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

            await _upcomingExpenseService.DeleteAsync(id, userId);

            return NoContent();
        }
    }
}