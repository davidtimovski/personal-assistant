﻿using System.Security.Claims;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Account.Controllers;

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
                string auth0Id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (_userIdLookup.Contains(auth0Id))
                {
                    userId = _userIdLookup.Get(auth0Id);
                }
                else
                {
                    var userId = _usersRepository.GetId(auth0Id);
                    if (!userId.HasValue)
                    {
                        throw new Exception($"The user with auth0_id '{auth0Id}' does not have a mapping");
                    }

                    _userIdLookup.Set(auth0Id, userId.Value);
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

            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }

    protected ITransaction StartTransactionWithUser(string name, string operation)
    {
        var tr = SentrySdk.StartTransaction(name, operation);
        tr.User = new User { Id = UserId.ToString() };

        return tr;
    }
}
