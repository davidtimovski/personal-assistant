using System;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Categories.Models;

public class UpdateCategory : CreateCategory
{
    public int Id { get; set; }
}