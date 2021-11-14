using System;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Categories.Models
{
    public class UpdateCategory : CreateCategory
    {
        public int Id { get; set; }
    }
}
