using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record BulkCreateRequest([Required] int ListId, [Required] string TasksText, [Required] bool TasksAreOneTime, [Required] bool TasksArePrivate);
