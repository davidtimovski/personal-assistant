import { AmountByCategory } from "./amountByCategory";
import { UpcomingExpenseDashboard } from "./upcomingExpenseDashboard";
import { DebtDashboard } from "./debtDashboard";

export class DashboardModel {
  constructor(
    public upcomingSum: number,
    public expenditures: Array<AmountByCategory>,
    public upcomingExpenses: Array<UpcomingExpenseDashboard>,
    public debt: Array<DebtDashboard>
  ) {}
}
