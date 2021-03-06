﻿using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Categories.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions.Models;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Mappings
{
    public class AccountantProfile : Profile
    {
        public AccountantProfile()
        {
            CreateMap<CreateAccount, Account>()
                .ForMember(x => x.Id, src => src.Ignore());
            CreateMap<CreateMainAccount, Account>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.IsMain, opt => opt.Ignore())
                .ForMember(x => x.Currency, src => src.Ignore())
                .ForMember(x => x.StockPrice, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore());
            CreateMap<UpdateAccount, Account>()
                .ForMember(x => x.IsMain, src => src.Ignore());

            CreateMap<CreateCategory, Category>()
                .ForMember(x => x.Id, src => src.Ignore());
            CreateMap<UpdateCategory, Category>();

            CreateMap<CreateTransaction, Transaction>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.FromAccount, src => src.Ignore())
                .ForMember(x => x.ToAccount, src => src.Ignore())
                .ForMember(x => x.Category, src => src.Ignore());
            CreateMap<UpdateTransaction, Transaction>()
                .ForMember(x => x.FromAccount, src => src.Ignore())
                .ForMember(x => x.ToAccount, src => src.Ignore())
                .ForMember(x => x.Category, src => src.Ignore());

            CreateMap<CreateUpcomingExpense, UpcomingExpense>()
                .ForMember(x => x.Id, src => src.Ignore());
            CreateMap<UpdateUpcomingExpense, UpcomingExpense>();

            CreateMap<CreateDebt, Debt>()
                .ForMember(x => x.Id, src => src.Ignore());
            CreateMap<UpdateDebt, Debt>();
        }
    }
}
