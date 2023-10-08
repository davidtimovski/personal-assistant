using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record SetShareIsAcceptedRequest([Required] int ListId, [Required] bool IsAccepted);
