using Accountant.Application.Contracts.Transactions.Models;
using AutoMapper;
using Application.Domain.Accountant;

namespace Accountant.Application.Mappings;

public class AccountantProfile : Profile
{
    public AccountantProfile()
    {
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
    }
}
