using System;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Categories.Models
{
    public class CreateCategory
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public CategoryType Type { get; set; }
        public bool GenerateUpcomingExpense { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
