namespace Jobs.Models.Accountant;

public sealed class Transaction
{
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; init; }
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public bool Generated { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
