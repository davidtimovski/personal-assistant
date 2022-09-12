import type { AmountByCategory } from './amountByCategory';
import type { DebtDashboard } from './debtDashboard';
import type { UpcomingExpenseDashboard } from './upcomingExpenseDashboard';

export class HomePageData {
	available = 0;
	spent = 0;
	balance = 0;
	upcomingSum = 0;
	expenditures: AmountByCategory[] | null = null;
	upcomingExpenses: UpcomingExpenseDashboard[] | null = null;
	debt: DebtDashboard[] | null = null;
}
