namespace Accountant.Domain

open System

module Models =
    type EntityType =
       | Category = 0
       | Account = 1
       | Transaction = 2
       | UpcomingExpense = 3
       | Debt = 4
       | AutomaticTransaction = 5

    type CategoryType =
       | AllTransactions = 0
       | DepositOnly = 1
       | WithdrawalOnly = 2

    type Account =
        { Id: int
          UserId: int
          Name: string
          IsMain: bool
          Currency: string
          StockPrice: decimal Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type Transaction =
        { Id: int
          FromAccountId: int Option
          ToAccountId: int Option
          CategoryId: int Option
          Amount: decimal
          FromStocks: decimal Option
          ToStocks: decimal Option
          Currency: string
          Description: string Option
          Date: DateTime
          IsEncrypted: bool
          EncryptedDescription: byte[] Option
          Salt: byte[] Option
          Nonce: byte[] Option
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type AutomaticTransaction =
        { Id: int
          UserId: int
          IsDeposit: bool
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          DayInMonth: int16
          CreatedDate: DateTime
          ModifiedDate: DateTime }

    type Category =
        { Id: int
          UserId: int
          ParentId: int Option
          Name: string
          Type: CategoryType
          GenerateUpcomingExpense: bool
          IsTax: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }

     type Debt =
        { Id: int
          UserId: int
          Person: string
          Amount: decimal
          Currency: string
          UserIsDebtor: bool
          Description: string Option
          CreatedDate: DateTime
          ModifiedDate: DateTime }

     type UpcomingExpense =
        { Id: int
          UserId: int
          CategoryId: int Option
          Amount: decimal
          Currency: string
          Description: string Option
          Date: DateTime
          Generated: bool
          CreatedDate: DateTime
          ModifiedDate: DateTime }
