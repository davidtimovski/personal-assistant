import { Category } from "models/entities/category";
import { Account } from "models/entities/account";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";

export class Changed {
  constructor(
    public lastSynced: string,
    public deletedAccountIds: Array<number>,
    public accounts: Array<Account>,
    public deletedCategoryIds: Array<number>,
    public categories: Array<Category>,
    public deletedTransactionIds: Array<number>,
    public transactions: Array<TransactionModel>,
    public deletedUpcomingExpenseIds: Array<number>,
    public upcomingExpenses: Array<UpcomingExpense>,
    public deletedDebtIds: Array<number>,
    public debts: Array<DebtModel>
  ) {}
}
