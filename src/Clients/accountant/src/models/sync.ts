import { Category } from "models/entities/category";
import { Account } from "models/entities/account";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";
import { AutomaticTransaction } from "models/entities/automaticTransaction";

export class Changed {
  lastSynced: string;
  deletedAccountIds: number[];
  accounts: Account[];
  deletedCategoryIds: number[];
  categories: Category[];
  deletedTransactionIds: number[];
  transactions: TransactionModel[];
  deletedUpcomingExpenseIds: number[];
  upcomingExpenses: UpcomingExpense[];
  deletedDebtIds: number[];
  debts: DebtModel[];
  deletedAutomaticTransactionIds: number[];
  automaticTransactions: AutomaticTransaction[];
}

export class Create {
  constructor(
    public accounts: Account[],
    public categories: Category[],
    public transactions: TransactionModel[],
    public upcomingExpenses: UpcomingExpense[],
    public debts: DebtModel[],
    public automaticTransactions: AutomaticTransaction[]
  ) {}
}

export class Created {
  accountIdPairs: CreatedIdPair[];
  categoryIdPairs: CreatedIdPair[];
  transactionIdPairs: CreatedIdPair[];
  upcomingExpenseIdPairs: CreatedIdPair[];
  debtIdPairs: CreatedIdPair[];
  automaticTransactionIdPairs: CreatedIdPair[];
}

export class CreatedIdPair {
  localId: number;
  id: number;
}
