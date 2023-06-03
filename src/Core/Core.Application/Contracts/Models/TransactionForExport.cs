namespace Core.Application.Contracts.Models;

public class TransactionForExport
{
    public decimal Amount { get; set; }
    public decimal? FromStocks { get; set; }
    public decimal? ToStocks { get; set; }
    public string Currency { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public bool IsEncrypted { get; set; }

    public AccountForExport FromAccount { get; set; } = null!;
    public AccountForExport ToAccount { get; set; } = null!;
    public CategoryForExport Category { get; set; } = null!;
}

public class CategoryForExport
{
    public string Name { get; set; } = null!;
}

public class AccountForExport
{
    public string Name { get; set; } = null!;
}
