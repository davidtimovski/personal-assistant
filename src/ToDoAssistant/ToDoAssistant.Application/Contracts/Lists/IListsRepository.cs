using ToDoAssistant.Application.Entities;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Application.Contracts.Lists;

public interface IListsRepository
{
    IReadOnlyList<ToDoList> GetAllAsOptions(int userId, ISpan metricsSpan);
    IReadOnlyList<int> GetNonArchivedSharedListIds(int userId);
    IReadOnlyList<ToDoList> GetAllWithTasksAndSharingDetails(int userId, ISpan metricsSpan);
    IReadOnlyList<User> GetMembersAsAssigneeOptions(int id, ISpan metricsSpan);
    ToDoList? Get(int id);
    ToDoList? GetWithShares(int id, int userId, ISpan metricsSpan);
    ToDoList? GetWithOwner(int id, int userId, ISpan metricsSpan);
    IReadOnlyList<ListShare> GetShares(int id, ISpan metricsSpan);
    IReadOnlyList<ListShare> GetShareRequests(int userId, ISpan metricsSpan);
    int GetPendingShareRequestsCount(int userId);
    bool CanShareWithUser(int shareWithId, int userId);
    bool UserOwnsOrShares(int id, int userId);
    bool UserOwnsOrSharesAsPending(int id, int userId);
    bool UserOwnsOrSharesAsAdmin(int id, int userId);
    bool UserOwnsOrSharesAsAdmin(int id, string name, int userId);
    bool UserOwns(int id, int userId);
    bool IsShared(int id);
    bool UserHasBlockedSharing(int listId, int userId, int sharedWithId);
    bool Exists(string name, int userId);
    bool Exists(int id, string name, int userId);
    int Count(int userId);
    IReadOnlyList<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, ISpan metricsSpan);
    IReadOnlyList<User> GetUsersToBeNotifiedOfDeletion(int id, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan);
    Task<int> CreateAsync(ToDoList list, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ToDoList> UpdateAsync(ToDoList list, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdateSharedAsync(ToDoList list, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<string> DeleteAsync(int id, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SaveSharingDetailsAsync(List<ListShare> newShares, List<ListShare> editedShares, List<ListShare> removedShares, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ListShare> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<int> CopyAsync(ToDoList list, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<bool> UncompleteAllAsync(int id, int userId, DateTime modifiedDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate, CancellationToken cancellationToken);
}
