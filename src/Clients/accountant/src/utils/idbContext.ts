import Dexie from "dexie";

import { Category } from "models/entities/category";
import { Account } from "models/entities/account";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";
import { AutomaticTransaction } from "models/entities/automaticTransaction";

export class IDBContext extends Dexie {
  public categories: Dexie.Table<Category, number>;
  public accounts: Dexie.Table<Account, number>;
  public transactions: Dexie.Table<TransactionModel, number>;
  public upcomingExpenses: Dexie.Table<UpcomingExpense, number>;
  public debts: Dexie.Table<DebtModel, number>;
  public automaticTransactions: Dexie.Table<AutomaticTransaction, number>;

  public constructor() {
    super("IDBContext");
    this.version(4).stores({
      categories: "id,parentId,type",
      accounts: "id",
      transactions: "id,fromAccountId,toAccountId,categoryId,date",
      upcomingExpenses: "id,categoryId,date",
      debts: "id",
      automaticTransactions: "id,categoryId",
    });
    this.categories = this.table("categories");
    this.categories.mapToClass(Category);

    this.accounts = this.table("accounts");
    this.transactions = this.table("transactions");
    this.transactions.mapToClass(TransactionModel);

    this.upcomingExpenses = this.table("upcomingExpenses");
    this.debts = this.table("debts");
    this.automaticTransactions = this.table("automaticTransactions");
  }
}
