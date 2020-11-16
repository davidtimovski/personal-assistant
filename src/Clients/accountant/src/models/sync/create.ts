import { Category } from "../entities/category";
import { Account } from "models/entities/account";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";

export class Create {
  constructor(
    public accounts: Array<Account>,
    public categories: Array<Category>,
    public transactions: Array<TransactionModel>,
    public upcomingExpenses: Array<UpcomingExpense>,
    public debts: Array<DebtModel>
  ) {}
}
