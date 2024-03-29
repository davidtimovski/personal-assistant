﻿namespace Jobs.Models.Accountant;

public class Transaction
{
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public bool Generated { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
