using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks
{
    public interface ITaskService
    {
        Task<SimpleTask> GetAsync(int id);
        Task<TaskDto> GetAsync(int id, int userId);
        Task<TaskForUpdate> GetForUpdateAsync(int id, int userId);
        bool Exists(int id, int userId);
        bool Exists(string name, int listId, int userId);
        bool Exists(IEnumerable<string> names, int listId, int userId);
        bool Exists(int id, string name, int listId, int userId);
        int Count(int listId);
        Task<CreatedTask> CreateAsync(CreateTask model, IValidator<CreateTask> validator);
        Task<IEnumerable<CreatedTask>> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator);
        Task UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator);
        Task<SimpleTask> DeleteAsync(int id, int userId);
        Task<SimpleTask> CompleteAsync(CompleteUncomplete model);
        Task<SimpleTask> UncompleteAsync(CompleteUncomplete model);
        Task ReorderAsync(ReorderTask model);
    }
}
