using Application.Domain.ToDoAssistant;
using Sentry;
using User = Application.Domain.Common.User;

namespace ToDoAssistant.Application.Contracts.Lists;

public interface IListsRepository
{
    IEnumerable<ToDoList> GetAllAsOptions(int userId, ISpan metricsSpan);
    IEnumerable<int> GetNonArchivedSharedListIds(int userId);
    IEnumerable<ToDoList> GetAllWithTasksAndSharingDetails(int userId, ISpan metricsSpan);
    IEnumerable<User> GetMembersAsAssigneeOptions(int id, ISpan metricsSpan);
    ToDoList Get(int id);
    ToDoList GetWithShares(int id, int userId, ISpan metricsSpan);
    ToDoList GetWithOwner(int id, int userId, ISpan metricsSpan);
    IEnumerable<ListShare> GetShares(int id, ISpan metricsSpan);
    IEnumerable<ListShare> GetShareRequests(int userId, ISpan metricsSpan);
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
    IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, ISpan metricsSpan);
    IEnumerable<User> GetUsersToBeNotifiedOfDeletion(int id, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan);
    Task<int> CreateAsync(ToDoList list, ISpan metricsSpan);
    Task<ToDoList> UpdateAsync(ToDoList list, int userId, ISpan metricsSpan);
    Task UpdateSharedAsync(ToDoList list, ISpan metricsSpan);
    Task<string> DeleteAsync(int id, ISpan metricsSpan);
    Task SaveSharingDetailsAsync(IEnumerable<ListShare> newShares, IEnumerable<ListShare> editedShares, IEnumerable<ListShare> removedShares, ISpan metricsSpan);
    Task<ListShare> LeaveAsync(int id, int userId, ISpan metricsSpan);
    Task<int> CopyAsync(ToDoList list, ISpan metricsSpan);
    Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate, ISpan metricsSpan);
    Task<bool> UncompleteAllAsync(int id, int userId, DateTime modifiedDate, ISpan metricsSpan);
    Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate, ISpan metricsSpan);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate);
}
