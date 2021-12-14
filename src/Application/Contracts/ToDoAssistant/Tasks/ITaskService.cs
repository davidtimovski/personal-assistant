using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks
{
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
        Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator);
        Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator);
        Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator);
        Task<DeleteTaskResult> DeleteAsync(int id, int userId);
        Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model);
        Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model);
        Task<ReorderTaskResult> ReorderAsync(ReorderTask model);
    }
}
