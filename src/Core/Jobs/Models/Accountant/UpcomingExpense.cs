﻿namespace Jobs.Models.Accountant;

public sealed class UpcomingExpense
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; init; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public bool Generated { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
