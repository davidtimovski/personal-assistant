using System;
using System.IO;
using System.Threading.Tasks;
using Api.Models.Accountant.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.Transactions.Models;
using Infrastructure.Identity;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IStringLocalizer<TransactionsController> _localizer;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TransactionsController(
        ITransactionService transactionService,
        IStringLocalizer<TransactionsController> localizer,
        IWebHostEnvironment webHostEnvironment)
    {
        _transactionService = transactionService;
        _localizer = localizer;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransaction dto)
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

        int id = await _transactionService.CreateAsync(dto);

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTransaction dto)
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

        await _transactionService.UpdateAsync(dto);

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

        await _transactionService.DeleteAsync(id, userId);

        return NoContent();
    }

    [HttpPost("export")]
    public IActionResult Export([FromBody] ExportVm vm)
    {
        if (vm == null)
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

        string directory = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp");
        var exportAsCsvModel = new ExportAsCsv(userId, directory, vm.FileId, _localizer["Uncategorized"], _localizer["Encrypted"]);

        FileStream file = _transactionService.ExportAsCsv(exportAsCsvModel);

        Response.Headers.Add("Content-Disposition", "attachment; filename=\"transactions.csv\"");
        return new FileStreamResult(file, "text/csv");
    }

    [HttpDelete("exported-file/{fileId}")]
    public IActionResult DeleteExportedFile(Guid fileId)
    {
        string directory = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp");
        var deleteExportedFileModel = new DeleteExportedFile(directory, fileId);

        _transactionService.DeleteExportedFile(deleteExportedFileModel);

        return NoContent();
    }
}
