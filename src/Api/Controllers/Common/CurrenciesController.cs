using System;
using System.Collections.Generic;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Common;

[Authorize]
[EnableCors("AllowAllApps")]
[Route("api/[controller]")]
public class CurrenciesController : Controller
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("{date}")]
    public IActionResult GetAll(DateTime date)
    {
        IDictionary<string, decimal> currencyRates = _currencyService.GetAll(date);

        return Json(currencyRates);
    }
}
