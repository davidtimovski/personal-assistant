using System;

namespace Domain.Entities.Accountant;

public class UpcomingExpense : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public bool Generated { get; set; }
}
