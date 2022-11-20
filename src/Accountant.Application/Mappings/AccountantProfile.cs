using Accountant.Application.Contracts.Accounts.Models;
using Accountant.Application.Contracts.AutomaticTransactions.Models;
using Accountant.Application.Contracts.Categories.Models;
using Accountant.Application.Contracts.Debts.Models;
using Accountant.Application.Contracts.Sync.Models;
using Accountant.Application.Contracts.Transactions.Models;
using Accountant.Application.Contracts.UpcomingExpenses.Models;
using AutoMapper;
using Domain.Accountant;

namespace Accountant.Application.Mappings;

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
        CreateMap<SyncAccount, Account>()
            .ForMember(x => x.IsMain, src => src.Ignore());

        CreateMap<CreateCategory, Category>()
            .ForMember(x => x.Id, src => src.Ignore());
        CreateMap<UpdateCategory, Category>();
        CreateMap<SyncCategory, Category>();

        CreateMap<CreateTransaction, Transaction>()
            .ForMember(x => x.Id, src => src.Ignore())
            .ForMember(x => x.Generated, src => src.Ignore())
            .ForMember(x => x.FromAccount, src => src.Ignore())
            .ForMember(x => x.ToAccount, src => src.Ignore())
            .ForMember(x => x.Category, src => src.Ignore());
        CreateMap<UpdateTransaction, Transaction>()
            .ForMember(x => x.Generated, src => src.Ignore())
            .ForMember(x => x.FromAccount, src => src.Ignore())
            .ForMember(x => x.ToAccount, src => src.Ignore())
            .ForMember(x => x.Category, src => src.Ignore());
        CreateMap<SyncTransaction, Transaction>()
            .ForMember(x => x.FromAccount, src => src.Ignore())
            .ForMember(x => x.ToAccount, src => src.Ignore())
            .ForMember(x => x.Category, src => src.Ignore());

        CreateMap<CreateUpcomingExpense, UpcomingExpense>()
            .ForMember(x => x.Id, src => src.Ignore());
        CreateMap<UpdateUpcomingExpense, UpcomingExpense>();
        CreateMap<SyncUpcomingExpense, UpcomingExpense>();

        CreateMap<CreateDebt, Debt>()
            .ForMember(x => x.Id, src => src.Ignore());
        CreateMap<UpdateDebt, Debt>();
        CreateMap<SyncDebt, Debt>();

        CreateMap<CreateAutomaticTransaction, AutomaticTransaction>()
            .ForMember(x => x.Id, src => src.Ignore());
        CreateMap<UpdateAutomaticTransaction, AutomaticTransaction>();
        CreateMap<SyncAutomaticTransaction, AutomaticTransaction>();
    }
}
