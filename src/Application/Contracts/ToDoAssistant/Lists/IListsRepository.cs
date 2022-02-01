using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.ToDoAssistant.Lists
{
    public interface IListsRepository
    {
        IEnumerable<ToDoList> GetAllAsOptions(int userId);
        IEnumerable<int> GetNonArchivedSharedListIds(int userId);
        IEnumerable<ToDoList> GetAllWithTasksAndSharingDetails(int userId);
        IEnumerable<User> GetMembersAsAssigneeOptions(int id);
        ToDoList Get(int id);
        ToDoList GetWithShares(int id, int userId);
        ToDoList GetWithOwner(int id, int userId);
        IEnumerable<ListShare> GetShares(int id);
        IEnumerable<ListShare> GetShareRequests(int userId);
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
        IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId);
        IEnumerable<User> GetUsersToBeNotifiedOfDeletion(int id);
        bool CheckIfUserCanBeNotifiedOfChange(int id, int userId);
        Task<int> CreateAsync(ToDoList list);
        Task<ToDoList> UpdateAsync(ToDoList list, int userId);
        Task UpdateSharedAsync(ToDoList list);
        Task<string> DeleteAsync(int id);
        Task SaveSharingDetailsAsync(IEnumerable<ListShare> newShares, IEnumerable<ListShare> editedShares, IEnumerable<ListShare> removedShares);
        Task<ListShare> LeaveAsync(int id, int userId);
        Task<int> CopyAsync(ToDoList list);
        Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate);
        Task<bool> SetTasksAsNotCompletedAsync(int id, int userId, DateTime modifiedDate);
        Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate);
        Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate);
    }
}
