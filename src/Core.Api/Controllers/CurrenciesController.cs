﻿using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

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
