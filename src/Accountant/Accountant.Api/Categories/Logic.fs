namespace Accountant.Api.Categories

open Accountant.Persistence.Fs.Models
open Models
open CommonHandlers

module Logic =
    [<Literal>]
    let private nameMaxLength = 30

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

    let private validateCreateParentCategory (dto: CreateCategory) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if
            dto.ParentId.IsNone
            || Validation.categoryBelongsTo dto.ParentId.Value userId connectionString
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateCreateName (dto: CreateCategory) =
        if
            (Validation.textIsNotEmpty dto.Name)
            && (Validation.textLengthIsValid dto.Name nameMaxLength)
        then
            Success dto
        else
            Failure "Name is not valid"

    let validateCreate = validateCreateParentCategory >> bind validateCreateName

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

    let private validateUpdateCategory (dto: UpdateCategory) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if Validation.categoryBelongsTo dto.Id userId connectionString then
            Success dto
        else
            Failure "Category is not valid"

    let private validateUpdateParentCategory (dto: UpdateCategory) =
        let userId = getUserId dto.HttpContext
        let connectionString = getConnectionString dto.HttpContext

        if
            dto.ParentId.IsNone
            || Validation.categoryBelongsTo dto.ParentId.Value userId connectionString
        then
            Success dto
        else
            Failure "Category is not valid"

    let private validateUpdateName (dto: UpdateCategory) =
        if
            (Validation.textIsNotEmpty dto.Name)
            && (Validation.textLengthIsValid dto.Name nameMaxLength)
        then
            Success dto
        else
            Failure "Name is not valid"

    let validateUpdate =
        validateUpdateCategory
        >> bind validateUpdateParentCategory
        >> bind validateUpdateName

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
