using System.Threading.Tasks;
using Application.Contracts.Accountant.AutomaticTransactions;
using Application.Contracts.Accountant.AutomaticTransactions.Models;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class AutomaticTransactionsController : BaseController
{
    private readonly IAutomaticTransactionService _automaticTransactionService;

    public AutomaticTransactionsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IAutomaticTransactionService automaticTransactionService) : base(userIdLookup, usersRepository)
    {
        _automaticTransactionService = automaticTransactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAutomaticTransaction dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = CurrentUserId;

        int id = await _automaticTransactionService.CreateAsync(dto);

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAutomaticTransaction dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = CurrentUserId;

        await _automaticTransactionService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _automaticTransactionService.DeleteAsync(id, CurrentUserId);

        return NoContent();
    }
}
