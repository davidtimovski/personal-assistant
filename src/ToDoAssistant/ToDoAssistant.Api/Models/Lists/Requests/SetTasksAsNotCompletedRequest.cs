using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record SetTasksAsNotCompletedRequest([Required] int ListId);
