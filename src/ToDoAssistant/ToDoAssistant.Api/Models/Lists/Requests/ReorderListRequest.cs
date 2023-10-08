using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record ReorderListRequest([Required] int Id, [Required] short OldOrder, [Required] short NewOrder);
