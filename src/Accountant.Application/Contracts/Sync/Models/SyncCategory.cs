using Domain.Accountant;

namespace Accountant.Application.Contracts.Sync.Models;

public class SyncCategory
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; }
    public bool GenerateUpcomingExpense { get; set; }
    public bool IsTax { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
