﻿using Application.Mappings;
using AutoMapper;
using Domain.Accountant;

namespace Accountant.Application.Contracts.Debts.Models;

public class DebtDto : IMapFrom<Debt>
{
    public int Id { get; set; }
    public string Person { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public bool UserIsDebtor { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Debt, DebtDto>();
    }
}