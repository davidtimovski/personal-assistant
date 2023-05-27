export class LargeUpcomingExpense {
	amount: number | null = null;

	constructor(public name: string, public iconClass: string, public currency: string) {}
}

export class SummaryItem {
	children: SummaryItem[] = [];

	constructor(public contentHtml: string) {}
}
