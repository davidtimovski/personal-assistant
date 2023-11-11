using Core.Application.Contracts;
using FluentValidation;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists.Models;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Application.Contracts.Lists;

public interface IListService
{
    Result<IReadOnlyList<ListDto>> GetAll(int userId, ISpan metricsSpan);
    Result<IReadOnlyList<ToDoListOption>> GetAllAsOptions(int userId, ISpan metricsSpan);
    Result<IReadOnlyList<Assignee>?> GetMembersAsAssigneeOptions(int id, int userId, ISpan metricsSpan);
    Result<SimpleList> Get(int id);
    Result<EditListDto?> GetForEdit(int id, int userId, ISpan metricsSpan);
    Result<ListWithShares?> GetWithShares(int id, int userId, ISpan metricsSpan);
    Result<IReadOnlyList<ShareListRequest>> GetShareRequests(int userId, ISpan metricsSpan);
    Result<int> GetPendingShareRequestsCount(int userId);
    Result<bool> CanShareWithUser(int shareWithId, int userId);
    Result<bool> UserOwnsOrShares(int id, int userId);
    Result<bool> UserOwnsOrSharesAsPending(int id, int userId);
    Result<bool> UserOwnsOrSharesAsAdmin(int id, int userId);
    Result<bool> UserOwnsOrSharesAsAdmin(int id, string name, int userId);
    Result<bool?> IsShared(int id, int userId);
    Result<bool> Exists(string name, int userId);
    Result<bool> Exists(int id, string name, int userId);
    Result<int> Count(int userId);
    Result<IReadOnlyList<User>> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate, ISpan metricsSpan);
    Result<IReadOnlyList<User>> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId, ISpan metricsSpan);
    Result<bool> CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan);
    Task<ValidatedResult<int>> CreateAsync(CreateList model, IValidator<CreateList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<DeleteListResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult> ShareAsync(ShareList model, IValidator<ShareList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<LeaveListResult> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<ValidatedResult<int>> CopyAsync(CopyList model, IValidator<CopyList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> SetIsArchivedAsync(int id, int userId, bool isArchived, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SetTasksAsNotCompletedResult> UncompleteAllAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> ReorderAsync(int id, int userId, short oldOrder, short newOrder, CancellationToken cancellationToken);
}
