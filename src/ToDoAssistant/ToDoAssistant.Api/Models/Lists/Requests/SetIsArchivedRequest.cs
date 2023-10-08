using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record SetIsArchivedRequest([Required] int ListId, [Required] bool IsArchived);
