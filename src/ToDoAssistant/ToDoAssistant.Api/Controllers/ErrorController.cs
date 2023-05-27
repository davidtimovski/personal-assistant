using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ToDoAssistant.Api.Controllers;

public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;
    private readonly IStringLocalizer<ErrorController> _localizer;

    public ErrorController(
        ILogger<ErrorController> logger,
        IStringLocalizer<ErrorController> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    [Route("/error")]
    public IActionResult Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (context.Error is ValidationException validationException)
        {
            foreach (ValidationFailure error in validationException.Errors)
            {
                ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
            }

            return new UnprocessableEntityObjectResult(ModelState);
        }

        _logger.LogError($"An unexpected exception was caught by {nameof(ErrorController)}", context.Error);

        return StatusCode(500);
    }
}
