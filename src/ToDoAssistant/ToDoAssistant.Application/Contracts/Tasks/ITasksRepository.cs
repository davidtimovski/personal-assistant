using Sentry;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Tasks;

public interface ITasksRepository
{
    ToDoTask Get(int id);
    ToDoTask? Get(int id, int userId);
    ToDoTask? GetForUpdate(int id, int userId);
    IReadOnlyList<string> GetRecipes(int id, int userId);
    bool Exists(int id, int userId);
    bool Exists(string name, int listId, int userId);
    bool Exists(List<string> names, int listId, int userId);
    bool Exists(int id, string name, int listId, int userId);
    bool IsPrivate(int id, int userId);
    int Count(int listId);
    Task<int> CreateAsync(ToDoTask task, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<IReadOnlyList<ToDoTask>> BulkCreateAsync(List<ToDoTask> tasks, bool tasksArePrivate, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdateAsync(ToDoTask task, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task CompleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UncompleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate, CancellationToken cancellationToken);
}
