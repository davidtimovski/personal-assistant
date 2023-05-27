import { AmountByCategory } from "./amountByCategory";
import { UpcomingExpenseDashboard } from "./upcomingExpenseDashboard";
import { DebtDashboard } from "./debtDashboard";

export class DashboardModel {
  constructor(
    public upcomingSum: number,
    public expenditures: AmountByCategory[],
    public upcomingExpenses: UpcomingExpenseDashboard[],
    public debt: DebtDashboard[]
  ) {}
}
