import { AmountByCategory } from "./viewmodels/amountByCategory";
import { UpcomingExpenseDashboard } from "./viewmodels/upcomingExpenseDashboard";
import { DebtDashboard } from "./viewmodels/debtDashboard";

export class Capital {
  constructor(
    public balance: number,
    public spent: number,
    public upcoming: number,
    public available: number,
    public expenditures: Array<AmountByCategory>,
    public upcomingExpenses: Array<UpcomingExpenseDashboard>,
    public debt: Array<DebtDashboard>
  ) {}
}
