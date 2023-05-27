using Microsoft.AspNetCore.Mvc;

namespace Account.Web.Controllers;

public class ErrorController : Controller
{
    [HttpGet, HttpPost, Route("~/error")]
    public IActionResult Error()
    {
        return RedirectToAction(nameof(HomeController.Error), "Home", new { code = 404 });
    }
}
