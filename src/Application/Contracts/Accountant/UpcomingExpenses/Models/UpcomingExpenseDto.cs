﻿using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models
{
    public class UpcomingExpenseDto : IMapFrom<UpcomingExpense>
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool Generated { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpcomingExpense, UpcomingExpenseDto>();
        }
    }
}
