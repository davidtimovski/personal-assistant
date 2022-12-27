using Application.Domain.Common;
using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists.Models;

namespace ToDoAssistant.Application.Contracts.Lists;

public interface IListService
{
    string[] IconOptions { get; }

    IEnumerable<ListDto> GetAll(int userId);
    IEnumerable<ToDoListOption> GetAllAsOptions(int userId);
    IEnumerable<Assignee> GetMembersAsAssigneeOptions(int id, int userId);
    SimpleList Get(int id);
    EditListDto GetForEdit(int id, int userId);
    ListWithShares GetWithShares(int id, int userId);
    IEnumerable<ShareListRequest> GetShareRequests(int userId);
    int GetPendingShareRequestsCount(int userId);
    bool CanShareWithUser(int shareWithId, int userId);
    bool UserOwnsOrShares(int id, int userId);
    bool UserOwnsOrSharesAsPending(int id, int userId);
    bool UserOwnsOrSharesAsAdmin(int id, int userId);
    bool UserOwnsOrSharesAsAdmin(int id, string name, int userId);
    bool IsShared(int id, int userId);
    bool Exists(string name, int userId);
    bool Exists(int id, string name, int userId);
    int Count(int userId);
    IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate);
    IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId);
    bool CheckIfUserCanBeNotifiedOfChange(int id, int userId);
    Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator);
    Task CreateSampleAsync(int userId, Dictionary<string, string> translations);
    Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator);
    Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator);
    Task<DeleteListResult> DeleteAsync(int id, int userId);
    Task ShareAsync(ShareList model, IValidator<ShareList> validator);
    Task<LeaveListResult> LeaveAsync(int id, int userId);
    Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator);
    Task SetIsArchivedAsync(int id, int userId, bool isArchived);
    Task<SetTasksAsNotCompletedResult> UncompleteAllAsync(int id, int userId);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder);
}
