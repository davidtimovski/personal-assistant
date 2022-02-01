using System;

namespace Application.Contracts.Accountant.Transactions.Models
{
    public class UpdateTransaction : CreateTransaction
    {
        public int Id { get; set; }
    }
}
