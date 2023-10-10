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

    let private validateCreateParentCategory (request: CreateCategoryRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.ParentId.IsNone
            || Validation.categoryBelongsTo request.ParentId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateCreateName (request: CreateCategoryRequest) =
        if
            (Validation.textIsNotEmpty request.Name)
            && (Validation.textLengthIsValid request.Name nameMaxLength)
        then
            Success request
        else
            Failure "Name is not valid"

    let validateCreate = validateCreateParentCategory >> bind validateCreateName

    let prepareForCreate (request: CreateCategoryRequest) (userId: int) =
        { Id = 0
          UserId = userId
          ParentId = request.ParentId
          Name = request.Name.Trim()
          Type = request.Type
          GenerateUpcomingExpense =
            match request.Type with
            | CategoryType.DepositOnly -> false
            | _ -> request.GenerateUpcomingExpense
          IsTax = request.IsTax
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }

    let private validateUpdateCategory (request: UpdateCategoryRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if Validation.categoryBelongsTo request.Id userId connectionString then
            Success request
        else
            Failure "Category is not valid"

    let private validateUpdateParentCategory (request: UpdateCategoryRequest) =
        let userId = getUserId request.HttpContext
        let connectionString = getConnectionString request.HttpContext

        if
            request.ParentId.IsNone
            || Validation.categoryBelongsTo request.ParentId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateUpdateName (request: UpdateCategoryRequest) =
        if
            (Validation.textIsNotEmpty request.Name)
            && (Validation.textLengthIsValid request.Name nameMaxLength)
        then
            Success request
        else
            Failure "Name is not valid"

    let validateUpdate =
        validateUpdateCategory
        >> bind validateUpdateParentCategory
        >> bind validateUpdateName

    let prepareForUpdate (request: UpdateCategoryRequest) (userId: int) =
        { Id = request.Id
          UserId = userId
          ParentId = request.ParentId
          Name = request.Name.Trim()
          Type = request.Type
          GenerateUpcomingExpense =
            match request.Type with
            | CategoryType.DepositOnly -> false
            | _ -> request.GenerateUpcomingExpense
          IsTax = request.IsTax
          CreatedDate = request.CreatedDate
          ModifiedDate = request.ModifiedDate }
