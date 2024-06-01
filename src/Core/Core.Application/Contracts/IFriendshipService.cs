using Core.Application.Contracts.Models;
using Core.Application.Entities;
using Sentry;

namespace Core.Application.Contracts;

public interface IFriendshipService
{
    Result<IReadOnlyList<FriendshipItemDto>> GetAll(int userId, ISpan metricsSpan);
    Result<FriendshipDto?> Get(int userId, int friendId, ISpan metricsSpan);
    Task<ValidatedResult> CreateAsync(CreateFriendship model, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> UpdateAsync(UpdateFriendship model, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> AcceptAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> DeclineAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> DeleteAsync(int userId, int friendId, ISpan metricsSpan, CancellationToken cancellationToken);
}
