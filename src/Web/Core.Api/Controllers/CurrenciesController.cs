using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CurrenciesController : Controller
{
    private readonly ICurrenciesRepository _currenciesRepository;

    public CurrenciesController(ICurrenciesRepository currenciesRepository)
    {
        _currenciesRepository = currenciesRepository;
    }

    [HttpGet("{date}")]
    public IActionResult GetAll(DateTime date)
    {
        var tr = SentrySdk.StartTransaction(
            "GET /api/currencies/{date}",
            $"{nameof(CurrenciesController)}.{nameof(GetAll)}"
        );

        IDictionary<string, decimal> currencyRates = _currenciesRepository.GetAll(date, tr);

        tr.Finish();

        return Json(currencyRates);
    }
}
