namespace Accountant.Application.Fs.Services

open System.Collections.Generic
open Application.Domain.Accountant
open Accountant.Application.Fs.Models.Categories

module CategoryService =
    let mapAll (categories: IEnumerable<Category>) : seq<CategoryDto> =
        categories
        |> Seq.map (fun x ->
            { Id = x.Id
              ParentId = x.ParentId
              Name = x.Name
              Type = x.Type
              GenerateUpcomingExpense = x.GenerateUpcomingExpense
              IsTax = x.IsTax
              CreatedDate = x.CreatedDate
              ModifiedDate = x.ModifiedDate })

    let prepareForCreate (model: CreateCategory) (userId: int) : Category =
        Category(
            Id = 0,
            UserId = userId,
            ParentId = model.ParentId,
            Name = model.Name.Trim(),
            Type = model.Type,
            GenerateUpcomingExpense = (if model.Type = CategoryType.DepositOnly then false else model.GenerateUpcomingExpense),
            IsTax = model.IsTax,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )

    let prepareForUpdate (model: UpdateCategory) (userId: int) : Category =
        Category(
            Id = model.Id,
            UserId = userId,
            ParentId = model.ParentId,
            Name = model.Name.Trim(),
            Type = model.Type,
            GenerateUpcomingExpense = (if model.Type = CategoryType.DepositOnly then false else model.GenerateUpcomingExpense),
            IsTax = model.IsTax,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate
        )
