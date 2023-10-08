using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record UpdateListRequest([Required] int Id, [Required] string Name, [Required] string Icon, [Required] bool IsOneTimeToggleDefault, [Required] bool NotificationsEnabled);
