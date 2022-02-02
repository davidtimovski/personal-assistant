using System;

namespace Application.Contracts.Accountant.Debts.Models;

public class UpdateDebt : CreateDebt
{
    public int Id { get; set; }
}