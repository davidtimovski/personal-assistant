namespace Domain.Entities.Accountant;

public class Category : Entity
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; }
    public bool GenerateUpcomingExpense { get; set; }
    public bool IsTax { get; set; }
}

public enum CategoryType
{
    AllTransactions,
    DepositOnly,
    WithdrawalOnly
}
