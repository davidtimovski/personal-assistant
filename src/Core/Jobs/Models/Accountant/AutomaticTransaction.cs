namespace Jobs.Models.Accountant;

internal class AutomaticTransaction
{
    internal int UserId { get; set; }
    internal bool IsDeposit { get; set; }
    internal int? CategoryId { get; set; }
    internal decimal Amount { get; set; }
    internal string Currency { get; set; }
    internal string Description { get; set; }
}
