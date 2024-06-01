using Core.Application.Entities;
using Sentry;

namespace Core.Application.Contracts;

public interface IFriendshipsRepository
{
    IReadOnlyList<Friendship> GetAll(int userId, ISpan metricsSpan);
    Friendship? Get(int userId1, int userId2, ISpan metricsSpan);
    bool Exists(int userId1, int userId2);
    Task CreateAsync(Friendship friendship, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdateAsync(Friendship friendship, ISpan metricsSpan, CancellationToken cancellationToken);
    Task AcceptAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeclineAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken);
    //Tooltip GetByKey(int userId, string key, string application, ISpan metricsSpan);
    //Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken);
}
