using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists
{
    public interface IListService
    {
        Task<IEnumerable<ListDto>> GetAllAsync(int userId);
        Task<IEnumerable<ToDoListOption>> GetAllAsOptionsAsync(int userId);
        Task<IEnumerable<AssigneeOption>> GetMembersAsAssigneeOptionsAsync(int id);
        Task<SimpleList> GetAsync(int id);
        Task<EditListDto> GetAsync(int id, int userId);
        Task<ListWithShares> GetWithSharesAsync(int id, int userId);
        Task<IEnumerable<ShareRequest>> GetShareRequestsAsync(int userId);
        Task<int> GetPendingShareRequestsCountAsync(int userId);
        Task<bool> CanShareWithUserAsync(int shareWithId, int userId);
        Task<bool> UserOwnsOrSharesAsync(int id, int userId);
        Task<bool> UserOwnsOrSharesAsPendingAsync(int id, int userId);
        Task<bool> UserOwnsOrSharesAsAdminAsync(int id, int userId);
        Task<bool> UserOwnsOrSharesAsAdminAsync(int id, string name, int userId);
        Task<bool> UserOwnsAsync(int id, int userId);
        Task<bool> IsSharedAsync(int id, int userId);
        Task<bool> UserHasBlockedSharingAsync(int userId, int sharedWithId);
        Task<bool> ExistsAsync(string name, int userId);
        Task<bool> ExistsAsync(int id, string name, int userId);
        Task<int> CountAsync(int userId);
        Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator);
        Task CreateSampleAsync(int userId, Dictionary<string, string> translations);
        Task<UpdateListOriginal> UpdateAsync(UpdateList model, IValidator<UpdateList> validator);
        Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator);
        Task<string> DeleteAsync(int id, int userId);
        Task ShareAsync(ShareList model, IValidator<ShareList> validator);
        Task<bool> LeaveAsync(int id, int userId);
        Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator);
        Task SetIsArchivedAsync(int id, int userId, bool isArchived);
        Task<bool> SetTasksAsNotCompletedAsync(int id, int userId);
        Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted);
        Task ReorderAsync(int id, int userId, short oldOrder, short newOrder);
    }
}
