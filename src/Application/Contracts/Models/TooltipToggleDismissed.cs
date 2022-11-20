using System.Linq;
using Application;
using Application.Contracts;

namespace Application.Contracts.Models;

public class TooltipToggleDismissed
{
    public string Key { get; set; }
    public string Application { get; set; }
    public bool IsDismissed { get; set; }
}