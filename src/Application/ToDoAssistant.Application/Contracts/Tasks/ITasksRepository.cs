using Application.Domain.ToDoAssistant;
using Sentry;

namespace ToDoAssistant.Application.Contracts.Tasks;

public interface ITasksRepository
{
    ToDoTask Get(int id);
    ToDoTask Get(int id, int userId);
    ToDoTask GetForUpdate(int id, int userId);
    List<string> GetRecipes(int id, int userId);
    bool Exists(int id, int userId);
    bool Exists(string name, int listId, int userId);
    bool Exists(List<string> names, int listId, int userId);
    bool Exists(int id, string name, int listId, int userId);
    bool IsPrivate(int id, int userId);
    int Count(int listId);
    Task<int> CreateAsync(ToDoTask task, int userId, ITransaction tr);
    Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId, ITransaction tr);
    Task UpdateAsync(ToDoTask task, int userId, ITransaction tr);
    Task DeleteAsync(int id, int userId, ITransaction tr);
    Task CompleteAsync(int id, int userId, ITransaction tr);
    Task UncompleteAsync(int id, int userId, ITransaction tr);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate);
}
