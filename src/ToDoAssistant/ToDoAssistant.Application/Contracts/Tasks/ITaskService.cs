using Core.Application.Contracts;
using FluentValidation;
using Sentry;
using ToDoAssistant.Application.Contracts.Tasks.Models;

namespace ToDoAssistant.Application.Contracts.Tasks;

public interface ITaskService
{
    Result<SimpleTask> Get(int id);
    Result<TaskDto?> Get(int id, int userId);
    Result<TaskForUpdate?> GetForUpdate(int id, int userId);
    Result<bool> Exists(int id, int userId);
    Result<bool> Exists(string name, int listId, int userId);
    Result<bool> Exists(List<string> names, int listId, int userId);
    Result<bool> Exists(int id, string name, int listId, int userId);
    Result<int> Count(int listId);
    Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<DeleteTaskResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model, ISpan metricsSpan, CancellationToken cancellationToken);
    //Task<ReorderTaskResult> ReorderAsync(ReorderTask model, ISpan metricsSpan, CancellationToken cancellationToken);
}
