using Application.Mappings;
using AutoMapper;
using Domain.Accountant;

namespace Accountant.Application.Contracts.AutomaticTransactions.Models;

public class AutomaticTransactionDto : IMapFrom<AutomaticTransaction>
{
    public int Id { get; set; }
    public bool IsDeposit { get; set; }
    public int? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public short DayInMonth { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<AutomaticTransaction, AutomaticTransactionDto>();
    }
}
