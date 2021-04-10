using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks
{
    public interface ITasksRepository
    {
        Task<ToDoTask> GetAsync(int id);
        Task<ToDoTask> GetAsync(int id, int userId);
        Task<ToDoTask> GetForUpdateAsync(int id, int userId);
        Task<List<string>> GetRecipesAsync(int id, int userId);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsAsync(string name, int listId, int userId);
        Task<bool> ExistsAsync(List<string> names, int listId, int userId);
        Task<bool> ExistsAsync(int id, string name, int listId, int userId);
        Task<bool> IsPrivateAsync(int id, int userId);
        Task<int> CountAsync(int listId);
        Task<int> CreateAsync(ToDoTask task, int userId);
        Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId);
        Task UpdateAsync(ToDoTask task, int userId);
        Task<ToDoTask> DeleteAsync(int id, int userId);
        Task<ToDoTask> CompleteAsync(int id, int userId);
        Task<ToDoTask> UncompleteAsync(int id, int userId);
        Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate);
    }
}
