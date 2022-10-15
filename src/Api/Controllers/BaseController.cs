using Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class BaseController : Controller
{
    private readonly IUserIdLookup _userIdLookup;
    private readonly IUsersRepository _usersRepository;

    public BaseController(IUserIdLookup userIdLookup, IUsersRepository usersRepository)
    {
        _userIdLookup = userIdLookup;
        _usersRepository = usersRepository;
    }

    private int? currentUserId;
    protected int CurrentUserId
    {
        get
        {
            if (!currentUserId.HasValue)
            {
                string auth0Id = User.FindFirst("sub").Value;
                
                if (_userIdLookup.Contains(auth0Id))
                {
                    currentUserId = _userIdLookup.Get(auth0Id);
                }
                else
                {
                    var userId = _usersRepository.GetId(auth0Id);
                    _userIdLookup.Set(auth0Id, userId);
                    currentUserId = userId;
                }                
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
