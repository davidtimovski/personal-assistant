using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Tasks.Requests;

public record CompleteUncompleteRequest([Required] int Id);
