using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record CreateListRequest([Required] string Name, [Required] string Icon, [Required] bool IsOneTimeToggleDefault, [Required] string? TasksText);
