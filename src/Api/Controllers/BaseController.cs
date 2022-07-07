using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class BaseController : Controller
{
    private int? currentUserId;
    protected int CurrentUserId
    {
        get
        {
            if (!currentUserId.HasValue)
            {
                string id = User.FindFirst("sub").Value;
                currentUserId = int.Parse(id);
            }

            return currentUserId.Value;
        }
    }

    private string currentUserName;
    protected string CurrentUserName
    {
        get
        {
            if (currentUserName == null)
            {
                currentUserName = User.FindFirst("name2").Value;
            }

            return currentUserName;
        }
    }
}
