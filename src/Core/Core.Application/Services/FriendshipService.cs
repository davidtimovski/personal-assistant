using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Entities;
using Core.Application.Utils;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Core.Application.Services;

public class FriendshipService : IFriendshipService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IFriendshipsRepository _friendshipsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(
        IUsersRepository? usersRepository,
        IFriendshipsRepository? friendshipsRepository,
        IMapper? mapper,
        ILogger<FriendshipService>? logger)
    {
        _usersRepository = ArgValidator.NotNull(usersRepository);
        _friendshipsRepository = ArgValidator.NotNull(friendshipsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public Result<IReadOnlyList<FriendshipItemDto>> GetAll(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(GetAll)}");

        try
        {
            var friendships = _friendshipsRepository.GetAll(userId, metric);

            var result = friendships.Select(x =>
            {
                var friend = userId == x.SenderId ? x.Recipient : x.Sender;
                return new FriendshipItemDto
                {
                    UserId = userId == x.SenderId ? x.RecipientId : x.SenderId,
                    Name = friend!.Name,
                    Email = friend.Email,
                    ImageUri = friend.ImageUri,
                    IsAccepted = x.IsAccepted,
                    UserIsSender = userId == x.SenderId
                };
            }).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<FriendshipDto?> Get(int userId, int friendId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(Get)}");

        try
        {
            var friendship = _friendshipsRepository.Get(userId, friendId, metric);
            if (friendship is null)
            {
                return new(null);
            }

            var friend = userId == friendship.SenderId ? friendship.Recipient : friendship.Sender;
            var result = new FriendshipDto
            {
                UserId = userId == friendship.SenderId ? friendship.RecipientId : friendship.SenderId,
                Name = friend!.Name,
                Email = friend.Email,
                ImageUri = friend.ImageUri,
                Permissions = friendship.Permissions.ToHashSet(),
                IsAccepted = friendship.IsAccepted,
                UserIsSender = userId == friendship.SenderId
            };

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> CreateAsync(CreateFriendship model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(CreateAsync)}");

        try
        {
            var user = _usersRepository.Get(model.Email.Trim());

            if (user.Id == model.UserId)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(model.Email), "CannotAddYourself")
                });
            }

            var friendshipExists = _friendshipsRepository.Exists(model.UserId, user.Id);
            if (friendshipExists)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(model.Email), "FriendshipAlreadyExists")
                });
            }

            // TODO: Create

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> UpdateAsync(UpdateFriendship model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(UpdateAsync)}");

        try
        {
            if (model.UserId == model.FriendId)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(null, "InvalidOperation")
                });
            }

            var friendshipExists = _friendshipsRepository.Exists(model.UserId, model.FriendId);
            if (!friendshipExists)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(null, "InvalidOperation")
                });
            }

            // TODO: Update

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> AcceptAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(AcceptAsync)}");

        try
        {
            // Validate that the userId is the recipient_id
            // Validate that a pending friendship request exists

            if (false)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(null, "InvalidOperation")
                });
            }

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(AcceptAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> DeclineAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(DeclineAsync)}");

        try
        {
            // Validate that the userId is the recipient_id
            // Validate that a pending friendship request exists

            if (false)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(null, "InvalidOperation")
                });
            }

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeclineAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> DeleteAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipService)}.{nameof(DeleteAsync)}");

        try
        {
            // Validate that a friendship request exists

            if (false)
            {
                return new(new List<ValidationFailure>
                {
                    new ValidationFailure(null, "InvalidOperation")
                });
            }

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }
}
