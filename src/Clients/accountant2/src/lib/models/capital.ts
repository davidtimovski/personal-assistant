import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
import type { UpcomingExpenseDashboard } from '$lib/models/viewmodels/upcomingExpenseDashboard';
import type { DebtDashboard } from '$lib/models/viewmodels/debtDashboard';

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
