﻿using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Accounts.Models
{
    public class CreateMainAccount
    {
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
