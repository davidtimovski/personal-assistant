namespace Domain.Accountant;

public class AutomaticTransaction : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsDeposit { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public short DayInMonth { get; set; }
}
