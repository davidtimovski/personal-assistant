﻿using FluentValidation;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists.Models;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Application.Contracts.Lists;

public interface IListService
{
    IEnumerable<ListDto> GetAll(int userId, ISpan metricsSpan);
    IEnumerable<ToDoListOption> GetAllAsOptions(int userId, ISpan metricsSpan);
    IEnumerable<Assignee> GetMembersAsAssigneeOptions(int id, int userId, ISpan metricsSpan);
    SimpleList Get(int id);
    EditListDto? GetForEdit(int id, int userId, ISpan metricsSpan);
    ListWithShares? GetWithShares(int id, int userId, ISpan metricsSpan);
    IEnumerable<ShareListRequest> GetShareRequests(int userId, ISpan metricsSpan);
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
    IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate, ISpan metricsSpan);
    IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan);
    Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<DeleteListResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task ShareAsync(ShareList model, IValidator<ShareList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<LeaveListResult> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SetIsArchivedAsync(int id, int userId, bool isArchived, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SetTasksAsNotCompletedResult> UncompleteAllAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, ISpan metricsSpan, CancellationToken cancellationToken);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, CancellationToken cancellationToken);
}
