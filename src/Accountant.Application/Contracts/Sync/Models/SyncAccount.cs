﻿namespace Accountant.Application.Contracts.Sync.Models;

public class SyncAccount
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public bool IsMain { get; set; }
    public string Currency { get; set; }
    public decimal? StockPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}