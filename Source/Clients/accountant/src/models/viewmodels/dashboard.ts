import { ExpenditureByCategory } from "./expenditureByCategory";
import { UpcomingExpenseDashboard } from "./upcomingExpenseDashboard";
import { DebtDashboard } from "./debtDashboard";

export class DashboardModel {
  constructor(
    public upcomingSum: number,
    public expenditures: Array<ExpenditureByCategory>,
    public upcomingExpenses: Array<UpcomingExpenseDashboard>,
    public debt: Array<DebtDashboard>
  ) {}
}
