using System;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts.Models
{
    public class AccountDto : IMapFrom<Account>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public string Currency { get; set; }
        public decimal? StockPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Account, AccountDto>();
        }
    }
}
