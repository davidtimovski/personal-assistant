using Api.Common;
using Core.Api.Models.Tooltips.Requests;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TooltipsController : BaseController
{
    private readonly ITooltipService _tooltipService;

    public TooltipsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        ITooltipService tooltipService) : base(userIdLookup, usersRepository)
    {
        _tooltipService = tooltipService;
    }

    [HttpGet("application/{application}")]
    public IActionResult GetAll(string application)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips/application/{application}",
            $"{nameof(TooltipsController)}.{nameof(GetAll)}",
            UserId
        );

        try
        {
            var result = _tooltipService.GetAll(application, UserId, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("key/{key}/{application}")]
    public IActionResult GetByKey(string key, string application)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips/key/{key}/{application}",
            $"{nameof(TooltipsController)}.{nameof(GetByKey)}",
            UserId
        );

        try
        {
            var result = _tooltipService.GetByKey(UserId, key, application, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> ToggleDismissed([FromBody] TooltipToggleDismissedRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tooltips",
            $"{nameof(TooltipsController)}.{nameof(ToggleDismissed)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _tooltipService.ToggleDismissedAsync(UserId, request.Key, request.Application, request.IsDismissed, tr, cancellationToken);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }
}
