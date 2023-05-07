namespace Core.Application.Contracts.Models;

public class TransactionForExport
{
    public decimal Amount { get; set; }
    public decimal? FromStocks { get; set; }
    public decimal? ToStocks { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public bool IsEncrypted { get; set; }

    public AccountForExport FromAccount { get; set; }
    public AccountForExport ToAccount { get; set; }
    public CategoryForExport Category { get; set; }
}

public class CategoryForExport
{
    public string Name { get; set; }
}

public class AccountForExport
{
    public string Name { get; set; }
}
