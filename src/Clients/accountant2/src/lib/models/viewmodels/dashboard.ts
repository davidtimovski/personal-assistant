import type { AmountByCategory } from './amountByCategory';
import type { UpcomingExpenseDashboard } from './upcomingExpenseDashboard';
import type { DebtDashboard } from './debtDashboard';

export class DashboardModel {
	constructor(
		public upcomingSum: number,
		public expenditures: AmountByCategory[],
		public upcomingExpenses: UpcomingExpenseDashboard[],
		public debt: DebtDashboard[]
	) {}
}
