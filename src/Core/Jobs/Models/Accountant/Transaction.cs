namespace Jobs.Models.Accountant;

internal class Transaction
{
    internal int? FromAccountId { get; set; }
    internal int? ToAccountId { get; set; }
    internal int? CategoryId { get; set; }
    internal decimal Amount { get; set; }
    internal string Currency { get; set; }
    internal string Description { get; set; }
    internal DateTime Date { get; set; }
    internal bool Generated { get; set; }
    internal DateTime CreatedDate { get; set; }
    internal DateTime ModifiedDate { get; set; }
}
