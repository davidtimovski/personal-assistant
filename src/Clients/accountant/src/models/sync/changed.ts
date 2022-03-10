import { Category } from "models/entities/category";
import { Account } from "models/entities/account";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";

export class Changed {
  lastSynced: string;
  deletedAccountIds: Array<number>;
  accounts: Array<Account>;
  deletedCategoryIds: Array<number>;
  categories: Array<Category>;
  deletedTransactionIds: Array<number>;
  transactions: Array<TransactionModel>;
  deletedUpcomingExpenseIds: Array<number>;
  upcomingExpenses: Array<UpcomingExpense>;
  deletedDebtIds: Array<number>;
  debts: Array<DebtModel>;
}
