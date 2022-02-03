using System;

namespace Application.Contracts.Accountant.Accounts.Models;

public class UpdateAccount : CreateAccount
{
    public int Id { get; set; }
}