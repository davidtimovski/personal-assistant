using System.Globalization;
using Account.Web.ViewModels.Home;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Account.Web.Controllers;

public class HomeController : BaseController
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public HomeController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IUserService userService,
        IConfiguration configuration) : base(userIdLookup, usersRepository)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Index(IndexAlert alert = IndexAlert.None)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Overview));
        }

        return View(new IndexViewModel(alert));
    }

    [HttpGet]
    [Authorize]
    public IActionResult Overview(OverviewAlert alert = OverviewAlert.None)
    {
        var user = _userService.Get(UserId);

        var model = new OverviewViewModel
        (
            user.Name,
            new List<ClientApplicationViewModel>
            {
                new ClientApplicationViewModel("To Do Assistant", _configuration["Urls:ToDoAssistant"], "to-do-assistant"),
                new ClientApplicationViewModel("Accountant", _configuration["Urls:Accountant"], "accountant"),
                new ClientApplicationViewModel("Weatherman", _configuration["Urls:Weatherman"], "weatherman"),
                new ClientApplicationViewModel("Cooking Assistant", "cooking-assistant"),
            },
            alert
        );

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

    [HttpGet]
    [ActionName("weatherman")]
    public IActionResult Weatherman()
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
