namespace Core.Application.Entities;

public class Tooltip
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public bool IsDismissed { get; set; }
}
