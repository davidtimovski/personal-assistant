using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record ReorderTaskRequest([Required] int Id, [Required] short OldOrder, [Required] short NewOrder);
