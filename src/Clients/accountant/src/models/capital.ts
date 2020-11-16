import { TransactionModel } from "./entities/transaction";
import { UpcomingExpense } from "./entities/upcomingExpense";
import { DebtModel } from "./entities/debt";

export class Capital {
  constructor(
    public balance: number,
    public spent: number,
    public upcoming: number,
    public available: number,
    public transactions: Array<TransactionModel>,
    public upcomingExpenses: Array<UpcomingExpense>,
    public debt: Array<DebtModel>
  ) {}
}
