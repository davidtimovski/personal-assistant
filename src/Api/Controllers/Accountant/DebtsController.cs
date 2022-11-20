using Accountant.Application.Contracts.Debts;
using Accountant.Application.Contracts.Debts.Models;
using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class DebtsController : BaseController
{
    private readonly IDebtService _debtService;

    public DebtsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IDebtService debtService) : base(userIdLookup, usersRepository)
    {
        _debtService = debtService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDebt dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        int id = await _debtService.CreateAsync(dto);

        return StatusCode(201, id);
    }

    [HttpPost("merged")]
    public async Task<IActionResult> CreateMerged([FromBody] CreateDebt dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        int id = await _debtService.CreateMergedAsync(dto);

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDebt dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        await _debtService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _debtService.DeleteAsync(id, UserId);

        return NoContent();
    }
}
