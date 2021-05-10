using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists
{
    public interface IListsRepository
    {
        Task<IEnumerable<ToDoList>> GetAllAsOptionsAsync(int userId);
        Task<IEnumerable<ToDoList>> GetAllWithTasksAndSharingDetailsAsync(int userId);
        Task<IEnumerable<User>> GetMembersAsAssigneeOptionsAsync(int id);
        Task<ToDoList> GetAsync(int id);
        Task<ToDoList> GetAsync(int id, int userId);
        Task<ToDoList> GetWithOwnerAsync(int id, int userId);
        Task<IEnumerable<ListShare>> GetSharesAsync(int id);
        Task<IEnumerable<ListShare>> GetShareRequestsAsync(int userId);
        Task<int> GetPendingShareRequestsCountAsync(int userId);
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
        Task<int> CreateAsync(ToDoList list);
        Task<ToDoList> UpdateAsync(ToDoList list);
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
