﻿using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        IDictionary<string, decimal> currencyRates = _currenciesRepository.GetAll(date);

        return Json(currencyRates);
    }
}