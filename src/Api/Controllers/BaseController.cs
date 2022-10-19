using System;
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

    private int? userId;
    protected int UserId
    {
        get
        {
            if (!userId.HasValue)
            {
                string auth0Id = User.FindFirst("sub").Value;
                
                if (_userIdLookup.Contains(auth0Id))
                {
                    userId = _userIdLookup.Get(auth0Id);
                }
                else
                {
                    var userId = _usersRepository.GetId(auth0Id);
                    _userIdLookup.Set(auth0Id, userId);
                    this.userId = userId;
                }
            }

            return userId.Value;
        }
    }

    protected string AuthId
    {
        get
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                throw new Exception($"The {nameof(AuthId)} property is only available for authenticated users");
            }

            return User.FindFirst("sub").Value;
        }
    }

    private string currentUserName;
    protected string CurrentUserName
    {
        get
        {
            if (currentUserName == null)
            {
                currentUserName = User.FindFirst("name").Value;
            }

            return currentUserName;
        }
    }
}
