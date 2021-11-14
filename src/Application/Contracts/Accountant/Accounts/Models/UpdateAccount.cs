using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts.Models
{
    public class UpdateAccount : CreateAccount
    {
        public int Id { get; set; }
    }
}
