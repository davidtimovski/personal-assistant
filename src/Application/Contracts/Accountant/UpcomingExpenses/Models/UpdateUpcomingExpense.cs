using System;

namespace Application.Contracts.Accountant.UpcomingExpenses.Models
{
    public class UpdateUpcomingExpense : CreateUpcomingExpense
    {
        public int Id { get; set; }
    }
}
