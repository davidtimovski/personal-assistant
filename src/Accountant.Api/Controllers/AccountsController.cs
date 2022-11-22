using Accountant.Application.Contracts.Accounts;
using Accountant.Application.Contracts.Accounts.Models;
using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Accountant.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AccountsController : BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IAccountService accountService) : base(userIdLookup, usersRepository)
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

        dto.UserId = UserId;

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

        dto.UserId = UserId;

        await _accountService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _accountService.DeleteAsync(id, UserId);

        return NoContent();
    }
}
