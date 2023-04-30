using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CurrenciesController : BaseController
{
    private readonly ICurrenciesRepository _currenciesRepository;

    public CurrenciesController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        ICurrenciesRepository currenciesRepository) : base(userIdLookup, usersRepository)
    {
        _currenciesRepository = currenciesRepository;
    }

    [HttpGet("{date}")]
    public IActionResult GetAll(DateTime date)
    {
        var tr = StartTransactionWithUser(
            "GET api/currencies/{date}",
            $"{nameof(CurrenciesController)}.{nameof(GetAll)}"
        );

        IDictionary<string, decimal> currencyRates = _currenciesRepository.GetAll(date, tr);

        tr.Finish();

        return Json(currencyRates);
    }
}
