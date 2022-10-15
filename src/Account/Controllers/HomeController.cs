using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Account.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Account.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User?.Identity.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Overview));
        }

        return RedirectToAction(nameof(AccountController.Login), "Account");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Overview(OverviewAlert alert = OverviewAlert.None)
    {
        var language = CultureInfo.CurrentCulture.Name;

        var model = new OverviewViewModel
        {
            UserName = "Test User's Name",
            ClientApplications = new List<ClientApplicationViewModel>
            {
                new ClientApplicationViewModel("To Do Assistant", new Uri(_configuration["Urls:ToDoAssistant"] + $"/{language}"), "to-do-assistant"),
                new ClientApplicationViewModel("Cooking Assistant", new Uri(_configuration["Urls:CookingAssistant"] + $"/{language}"), "cooking-assistant"),
                new ClientApplicationViewModel("Accountant", new Uri(_configuration["Urls:Accountant2"] + $"?lang={language}"), "accountant")
            },
            Alert = alert
        };

        return View(model);
    }

    [HttpGet]
    [ActionName("privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View();
    }

    [HttpGet]
    [ActionName("about")]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    [ActionName("to-do-assistant")]
    public IActionResult ToDoAssistant()
    {
        return View();
    }

    [HttpGet]
    [ActionName("cooking-assistant")]
    public IActionResult CookingAssistant()
    {
        return View();
    }

    [HttpGet]
    [ActionName("accountant")]
    public IActionResult Accountant()
    {
        return View();
    }

    [HttpPost]
    [ActionName("change-language")]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeLanguage(string returnUrl)
    {
        var otherLanguage = CultureInfo.CurrentCulture.Name == "en-US" ? "mk-MK" : "en-US";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(otherLanguage)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(3),
                IsEssential = true,
                Path = "/",
                HttpOnly = false,
                SameSite = SameSiteMode.Strict
            }
        );
        Response.Cookies.Append(
            "LanguagePrompt",
            "false",
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(3),
                IsEssential = true,
                Path = "/",
                HttpOnly = false,
                SameSite = SameSiteMode.Strict
            }
        );

        if (returnUrl != null)
        {
            return RedirectToLocal(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Error(int code)
    {
        var model = new ErrorViewModel
        {
            ErrorCode = code
        };
        return View(model);
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
