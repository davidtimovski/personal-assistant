using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Domain.Entities.Common;

namespace Application.Contracts.ToDoAssistant.Lists;

public interface IListService
{
    IEnumerable<ListDto> GetAll(int userId);
    IEnumerable<ToDoListOption> GetAllAsOptions(int userId);
    IEnumerable<AssigneeOption> GetMembersAsAssigneeOptions(int id, int userId);
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
    Task<SetTasksAsNotCompletedResult> SetTasksAsNotCompletedAsync(int id, int userId);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder);
}