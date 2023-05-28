using System.Text.Json.Serialization;

namespace Core.Application.Contracts.Models;

public class TooltipToggleDismissed
{
    [JsonRequired]
    public string Key { get; set; } = null!;

    [JsonRequired]
    public string Application { get; set; } = null!;

    [JsonRequired]
    public bool IsDismissed { get; set; }
}
