using FluentValidation;
using Sentry;
using ToDoAssistant.Application.Contracts.Tasks.Models;

namespace ToDoAssistant.Application.Contracts.Tasks;

public interface ITaskService
{
    SimpleTask Get(int id);
    TaskDto Get(int id, int userId);
    TaskForUpdate GetForUpdate(int id, int userId);
    bool Exists(int id, int userId);
    bool Exists(string name, int listId, int userId);
    bool Exists(IEnumerable<string> names, int listId, int userId);
    bool Exists(int id, string name, int listId, int userId);
    int Count(int listId);
    Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator, ISpan metricsSpan);
    Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator, ISpan metricsSpan);
    Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator, ISpan metricsSpan);
    Task<DeleteTaskResult> DeleteAsync(int id, int userId, ISpan metricsSpan);
    Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model, ISpan metricsSpan);
    Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model, ISpan metricsSpan);
    Task<ReorderTaskResult> ReorderAsync(ReorderTask model, ISpan metricsSpan);
}
