using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

public abstract class BaseController : Controller
{
    private readonly IUserIdLookup _userIdLookup;
    private readonly IUsersRepository _usersRepository;

    public BaseController(IUserIdLookup? userIdLookup, IUsersRepository? usersRepository)
    {
        _userIdLookup = ArgValidator.NotNull(userIdLookup);
        _usersRepository = ArgValidator.NotNull(usersRepository);
    }

    private int? userId;
    protected int UserId
    {
        get
        {
            if (!userId.HasValue)
            {
                var auth0Id = User?.Identity?.Name;
                if (auth0Id is null)
                {
                    throw new InvalidOperationException($"{nameof(UserId)} invoked for non-authenticated user");
                }

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
}
