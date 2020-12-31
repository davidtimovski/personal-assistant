using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts.Models
{
    public class UpdateAccount
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal? StockPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
