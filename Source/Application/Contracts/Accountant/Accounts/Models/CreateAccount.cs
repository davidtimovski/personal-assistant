using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts.Models
{
    public class CreateAccount
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
