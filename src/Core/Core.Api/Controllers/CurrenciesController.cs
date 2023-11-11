using Api.Common;
using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CurrenciesController : BaseController
{
    private readonly ICurrenciesRepository _currenciesRepository;

    public CurrenciesController(
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        ICurrenciesRepository? currenciesRepository) : base(userIdLookup, usersRepository)
    {
        _currenciesRepository = ArgValidator.NotNull(currenciesRepository);
    }

    [HttpGet("{date}")]
    public IActionResult GetAll(DateTime date)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/currencies/{date}",
            $"{nameof(CurrenciesController)}.{nameof(GetAll)}",
            UserId
        );

        try
        {
            IDictionary<string, decimal> currencyRates = _currenciesRepository.GetAll(date, tr);
            return Json(currencyRates);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }
}
