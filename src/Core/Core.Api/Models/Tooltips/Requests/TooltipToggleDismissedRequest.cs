namespace Core.Api.Models.Tooltips.Requests;

public record TooltipToggleDismissedRequest(string Key, string Application, bool IsDismissed);
