namespace Jobs.Models.Accountant;

public sealed class AutomaticTransaction
{
    public int UserId { get; set; }
    public bool IsDeposit { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; init; }
    public string? Description { get; set; }
}
