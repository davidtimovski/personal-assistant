using System.Linq;

namespace Application.Contracts.Common.Models
{
    public class TooltipToggleDismissed
    {
        public string Key { get; set; }
        public string Application { get; set; }
        public bool IsDismissed { get; set; }
    }
}
