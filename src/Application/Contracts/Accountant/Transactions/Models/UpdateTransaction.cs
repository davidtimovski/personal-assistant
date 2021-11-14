using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Transactions.Models
{
    public class UpdateTransaction : CreateTransaction
    {
        public int Id { get; set; }
    }
}
