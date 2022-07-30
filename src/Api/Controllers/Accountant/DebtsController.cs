using System.Threading.Tasks;
using Application.Contracts.Accountant.Debts;
using Application.Contracts.Accountant.Debts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant,AllowAccountant2")]
[Route("api/[controller]")]
public class DebtsController : BaseController
{
    private readonly IDebtService _debtService;

    public DebtsController(IDebtService debtService)
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

        dto.UserId = CurrentUserId;

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

        dto.UserId = CurrentUserId;

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

        dto.UserId = CurrentUserId;

        await _debtService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _debtService.DeleteAsync(id, CurrentUserId);

        return NoContent();
    }
}
