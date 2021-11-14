using System;

namespace PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models
{
    public class UpdateUpcomingExpense : CreateUpcomingExpense
    {
        public int Id { get; set; }
    }
}
