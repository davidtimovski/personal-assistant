namespace Accountant.Application.Fs.Services

open Accountant.Domain.Models
open Accountant.Application.Fs.Models.Categories

module CategoryService =
    let mapAll (categories: seq<Category>) : seq<CategoryDto> =
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

    let prepareForCreate (model: CreateCategory) (userId: int) =
        { Id = 0
          UserId = userId
          ParentId = model.ParentId
          Name = model.Name.Trim()
          Type = model.Type
          GenerateUpcomingExpense =
            match model.Type with
            | CategoryType.DepositOnly -> false
            | _ -> model.GenerateUpcomingExpense
          IsTax = model.IsTax
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }

    let prepareForUpdate (model: UpdateCategory) (userId: int) =
        { Id = model.Id
          UserId = userId
          ParentId = model.ParentId
          Name = model.Name.Trim()
          Type = model.Type
          GenerateUpcomingExpense =
            match model.Type with
            | CategoryType.DepositOnly -> false
            | _ -> model.GenerateUpcomingExpense
          IsTax = model.IsTax
          CreatedDate = model.CreatedDate
          ModifiedDate = model.ModifiedDate }
