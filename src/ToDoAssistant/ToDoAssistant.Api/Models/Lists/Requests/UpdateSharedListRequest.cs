using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record UpdateSharedListRequest([Required] int Id, [Required] bool NotificationsEnabled);
