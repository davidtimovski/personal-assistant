using Core.Application.Contracts;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ToDoAssistant.Api.Controllers;

public abstract class BaseController : Controller
{
    private readonly IUserIdLookup _userIdLookup;
    private readonly IUsersRepository _usersRepository;
    private readonly IStringLocalizer<BaseController> _localizer;

    public BaseController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IStringLocalizer<BaseController> localizer)
    {
        _userIdLookup = userIdLookup;
        _usersRepository = usersRepository;
        _localizer = localizer;
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

    protected string AuthId
    {
        get
        {
            if (User?.Identity?.IsAuthenticated == false || User?.Identity?.Name is null)
            {
                throw new Exception($"The {nameof(AuthId)} property is only available for authenticated users");
            }

            return User.Identity.Name;
        }
    }

    protected IActionResult UnprocessableEntityResult(IReadOnlyList<ValidationFailure> validationErrors)
    {
        var result = new Dictionary<string, List<string>>();

        foreach (ValidationFailure error in validationErrors)
        {
            if (result.ContainsKey(error.PropertyName))
            {
                result[error.PropertyName].Add(_localizer[error.ErrorMessage]);
            }
            else
            {
                result.Add(error.PropertyName, new List<string> { _localizer[error.ErrorMessage] });
            }
        }

        return new UnprocessableEntityObjectResult(result);
    }
}
