using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Common
{
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
            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            IDictionary<string, decimal> currencyRates = _currencyService.GetAll(date);
            if (currencyRates == null)
            {
                return NotFound();
            }

            return Json(currencyRates);
        }
    }
}