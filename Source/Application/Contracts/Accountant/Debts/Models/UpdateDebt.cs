using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Debts.Models
{
    public class UpdateDebt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Person { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool UserIsDebtor { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
