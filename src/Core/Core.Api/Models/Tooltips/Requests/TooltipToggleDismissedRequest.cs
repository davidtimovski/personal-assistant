using System.ComponentModel.DataAnnotations;

namespace Core.Api.Models.Tooltips.Requests;

public record TooltipToggleDismissedRequest([Required] string Key, [Required] string Application, [Required] bool IsDismissed);
