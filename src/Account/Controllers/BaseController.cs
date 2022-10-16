using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Account.Controllers;

public abstract class BaseController : Controller
{
    protected string AuthId
    {
        get
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
