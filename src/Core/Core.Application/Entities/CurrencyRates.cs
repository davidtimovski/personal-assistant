namespace Core.Application.Entities;

public class CurrencyRates
{
    public DateTime Date { get; set; }
    public string Rates { get; set; } = null!;
}
