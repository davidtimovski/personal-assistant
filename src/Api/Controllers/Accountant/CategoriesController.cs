using System.Threading.Tasks;
using Application.Contracts.Accountant.Categories;
using Application.Contracts.Accountant.Categories.Models;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        ICategoryService categoryService) : base(userIdLookup, usersRepository)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategory dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        int id = await _categoryService.CreateAsync(dto);

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCategory dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        await _categoryService.UpdateAsync(dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id, UserId);

        return NoContent();
    }
}
