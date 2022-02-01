using System;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Sync.Models
{
    public class SyncCategory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public CategoryType Type { get; set; }
        public bool GenerateUpcomingExpense { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
