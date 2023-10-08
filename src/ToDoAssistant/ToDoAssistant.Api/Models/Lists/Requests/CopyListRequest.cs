using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record CopyListRequest([Required] int Id, [Required] string Name, [Required] string Icon);
