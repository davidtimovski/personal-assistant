namespace Application.Domain.Accountant;

public class Account : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public bool IsMain { get; set; }
    public string Currency { get; set; }
    public decimal? StockPrice { get; set; }
}
