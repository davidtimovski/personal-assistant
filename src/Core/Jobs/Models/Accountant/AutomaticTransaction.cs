namespace Jobs.Models.Accountant;

public class AutomaticTransaction
{
    public int UserId { get; set; }
    public bool IsDeposit { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string? Description { get; set; }
}
