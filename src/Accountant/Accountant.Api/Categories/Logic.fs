namespace Accountant.Api.Categories

open Accountant.Persistence.Models
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

        if request.ParentId.IsNone
            || Validation.categoryBelongsTo request.ParentId.Value userId connectionString
        then
            Success request
        else
            Failure "Category is not valid"

    let private validateCreateName (request: CreateCategoryRequest) =
        if (Validation.textIsNotEmpty request.Name)
            && (Validation.textLengthIsValid request.Name nameMaxLength)
        then
            Success request
        else
            Failure "Name is not valid"

    let validateCreate =
        validateCreateParentCategory
        >> bind validateCreateName

    let createRequestToEntity (request: CreateCategoryRequest) (userId: int) =
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

    type UpdateValidationParams =
        { CurrentUserId: int
          Request: UpdateCategoryRequest
          ExistingCategory: Category option
          ExistingParentCategory: Category option }

    let private validateUpdateCategory (parameters: UpdateValidationParams) =
        match parameters.ExistingCategory with
        | Some category ->
            if category.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Category does not belong to user"
        | None -> Failure "Category does not exist"

    let private validateUpdateParentCategory (parameters: UpdateValidationParams) =
        match parameters.ExistingParentCategory with
        | Some category ->
            if category.UserId = parameters.CurrentUserId then
                Success parameters
            else
                Failure "Parent category does not belong to user"
        | None -> Success parameters

    let private validateUpdateName (parameters: UpdateValidationParams) =
        if (Validation.textIsNotEmpty parameters.Request.Name)
            && (Validation.textLengthIsValid parameters.Request.Name nameMaxLength)
        then
            Success parameters
        else
            Failure "Name is not valid"

    let validateUpdate =
        validateUpdateCategory
        >> bind validateUpdateParentCategory
        >> bind validateUpdateName

    let updateRequestToEntity (request: UpdateCategoryRequest) (userId: int) =
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
