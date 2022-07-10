using System;

namespace Application.Contracts.Accountant.AutomaticTransactions.Models;

public class UpdateAutomaticTransaction : CreateAutomaticTransaction
{
    public int Id { get; set; }
}
