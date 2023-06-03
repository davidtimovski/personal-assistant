namespace Jobs.Models.Accountant;

internal class UpcomingExpense
{
    internal int Id { get; set; }
    internal int UserId { get; set; }
    internal int? CategoryId { get; set; }
    internal decimal Amount { get; set; }
    internal string Currency { get; set; } = null!;
    internal string? Description { get; set; }
    internal DateTime Date { get; set; }
    internal bool Generated { get; set; }
    internal DateTime CreatedDate { get; set; }
    internal DateTime ModifiedDate { get; set; }
}
