using Api.Common;
using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PersonalAssistant.Web.ViewModels.Friendships;
using Sentry;

namespace PersonalAssistant.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class FriendshipsController : BaseController
{
    private readonly IFriendshipService _friendshipService;
    private readonly IStringLocalizer<FriendshipsController> _localizer;

    public FriendshipsController(
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        IFriendshipService? friendshipService,
        IStringLocalizer<FriendshipsController>? localizer) : base(userIdLookup, usersRepository)
    {
        _friendshipService = ArgValidator.NotNull(friendshipService);
        _localizer = ArgValidator.NotNull(localizer);
    }

    [HttpGet]
    public IActionResult Index(FriendshipsIndexAlert alert = FriendshipsIndexAlert.None)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships",
            $"{nameof(FriendshipsController)}.{nameof(Index)}"
        );

        try
        {
            var result = _friendshipService.GetAll(UserId, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            var model = new IndexViewModel
            {
                Alert = alert,
                Friendships = result.Data!.Select(x => new FriendshipItemViewModel
                {
                    UserId = x.UserId,
                    Name = x.Name,
                    Email = x.Email,
                    ImageUri = x.ImageUri,
                    IsAccepted = x.IsAccepted,
                    UserIsSender = x.UserIsSender
                }).ToList()
            };

            return View(model);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("view/{userId}")]
    public IActionResult View(int userId)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/view",
            $"{nameof(FriendshipsController)}.{nameof(View)}"
        );

        try
        {
            var result = _friendshipService.Get(UserId, userId, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Data is null)
            {
                tr.Status = SpanStatus.NotFound;
                return View(); // TODO
            }

            var model = new ViewFriendshipViewModel
            {
                UserId = result.Data.UserId,
                Name = result.Data.Name,
                Email = result.Data.Email,
                ImageUri = result.Data.ImageUri,
                Permissions = result.Data.Permissions,
                IsAccepted = result.Data.IsAccepted,
                UserIsSender = result.Data.UserIsSender
            };

            return View(model);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("new")]
    public IActionResult New()
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/new",
            $"{nameof(FriendshipsController)}.{nameof(New)}"
        );

        try
        {
            var model = new CreateFriendshipViewModel
            {
                Email = string.Empty,
                Permissions = new List<string>
                {
                    "todo_share_lists",
                    "chef_share_recipes",
                    "chef_send_recipes"
                }
            };
            return View(model);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(CreateFriendshipViewModel request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/new",
            $"{nameof(FriendshipsController)}.{nameof(New)}"
        );

        try
        {
            if (!ModelState.IsValid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return View(request);
            }

            var model = new CreateFriendship
            {
                UserId = UserId,
                Email = request.Email,
                Permissions = request.Permissions.ToHashSet()
            };

            var result = await _friendshipService.CreateAsync(model, tr, cancellationToken);
            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                
                foreach (var error in result.ValidationErrors!)
                {
                    ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Index), "Friendships", new { alert = FriendshipsIndexAlert.FriendshipRequestSent });
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("edit/{userId}")]
    public IActionResult Edit(int userId)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/edit",
            $"{nameof(FriendshipsController)}.{nameof(Edit)}"
        );

        try
        {
            var result = _friendshipService.Get(UserId, userId, tr);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            var model = new ViewFriendshipViewModel
            {
                UserId = result.Data!.UserId,
                Name = result.Data.Name,
                Email = result.Data.Email,
                ImageUri = result.Data.ImageUri,
                Permissions = result.Data.Permissions,
                IsAccepted = result.Data.IsAccepted,
                UserIsSender = result.Data.UserIsSender
            };

            return View(model);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateFriendshipViewModel request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/edit",
            $"{nameof(FriendshipsController)}.{nameof(Edit)}"
        );

        try
        {
            if (!ModelState.IsValid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return View(request);
            }

            var model = new UpdateFriendship
            {
                UserId = UserId,
                FriendId = request.FriendId,
                Permissions = request.Permissions.ToHashSet()
            };

            var result = await _friendshipService.UpdateAsync(model, tr, cancellationToken);
            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;

                foreach (var error in result.ValidationErrors!)
                {
                    ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Index), "Friendships", new { alert = FriendshipsIndexAlert.FriendshipUpdated });
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(AcceptFriendshipRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/accept",
            $"{nameof(FriendshipsController)}.{nameof(Accept)}"
        );

        try
        {
            var result = await _friendshipService.AcceptAsync(UserId, request.FriendId, tr, cancellationToken);
            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;

                foreach (var error in result.ValidationErrors!)
                {
                    ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Index), "Friendships", new { alert = FriendshipsIndexAlert.FriendshipAccepted });
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("decline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(DeclineFriendshipRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/decline",
            $"{nameof(FriendshipsController)}.{nameof(Decline)}"
        );

        try
        {
            var result = await _friendshipService.DeclineAsync(UserId, request.FriendId, tr, cancellationToken);
            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;

                foreach (var error in result.ValidationErrors!)
                {
                    ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Index), "Friendships", new { alert = FriendshipsIndexAlert.FriendshipDeclined });
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeleteFriendshipRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransaction(
            $"{Request.Method} friendships/delete",
            $"{nameof(FriendshipsController)}.{nameof(Delete)}"
        );

        try
        {
            var result = await _friendshipService.DeleteAsync(UserId, request.FriendId, tr, cancellationToken);
            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return View(); // TODO
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;

                foreach (var error in result.ValidationErrors!)
                {
                    ModelState.AddModelError(error.PropertyName, _localizer[error.ErrorMessage]);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Index), "Friendships", new { alert = FriendshipsIndexAlert.FriendshipDeleted });
        }
        finally
        {
            tr.Finish();
        }
    }
}
