using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Common
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [EnableCors("AllowAllApps")]
    [Route("api/[controller]")]
    public class CurrenciesController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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

            string currencyRatesJson = await _currencyService.GetAllAsJsonAsync();

            return Ok(currencyRatesJson);
        }
    }
}