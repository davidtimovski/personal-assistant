import { AmountByCategory } from "./viewmodels/amountByCategory";
import { UpcomingExpenseDashboard } from "./viewmodels/upcomingExpenseDashboard";
import { DebtDashboard } from "./viewmodels/debtDashboard";

export class Capital {
  constructor(
    public balance: number,
    public spent: number,
    public upcoming: number,
    public available: number,
    public expenditures: AmountByCategory[],
    public upcomingExpenses: UpcomingExpenseDashboard[],
    public debt: DebtDashboard[]
  ) {}
}
