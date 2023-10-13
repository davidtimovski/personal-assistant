import type { AmountByCategory } from './amountByCategory';

export class HomePageData {
	available = 0;
	spent = 0;
	balance = 0;
	upcomingSum = 0;
	expenditures: AmountByCategory[] | null = null;
	upcomingExpenses: HomePageUpcomingExpense[] | null = null;
	debt: HomePageDebt[] | null = null;
}

export class HomePageDebt {
	constructor(public id: number, public person: string, public userIsDebtor: boolean, public description: string, public amount: number) {}
}

export class HomePageUpcomingExpense {
	constructor(public id: number, public category: string, public description: string, public amount: number) {}
}
