using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record CreateTaskRequest([Required] int ListId, [Required] string Name, [Required] string? Url, [Required] bool IsOneTime, [Required] bool? IsPrivate);
