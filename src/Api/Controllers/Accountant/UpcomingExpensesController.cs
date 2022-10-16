﻿using System.Threading.Tasks;
using Application.Contracts.Accountant.UpcomingExpenses;
using Application.Contracts.Accountant.UpcomingExpenses.Models;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class UpcomingExpensesController : BaseController
{
    private readonly IUpcomingExpenseService _upcomingExpenseService;

    public UpcomingExpensesController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IUpcomingExpenseService upcomingExpenseService) : base(userIdLookup, usersRepository)
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

        dto.UserId = UserId;

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

        dto.UserId = UserId;

        await _upcomingExpenseService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _upcomingExpenseService.DeleteAsync(id, UserId);

        return NoContent();
    }
}
