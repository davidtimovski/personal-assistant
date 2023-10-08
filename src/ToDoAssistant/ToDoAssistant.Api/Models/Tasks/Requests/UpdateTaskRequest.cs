using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record UpdateTaskRequest([Required] int Id, [Required] int ListId, [Required] string Name, [Required] string? Url, [Required] bool IsOneTime, [Required] bool IsHighPriority, [Required] bool? IsPrivate, [Required] int? AssignedToUserId);
