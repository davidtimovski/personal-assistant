import type { AmountByCategory } from './amountByCategory';

export class PieChartItem {
	color: string | null = null;
	subItems: PieChartItem[];

	constructor(public data: AmountByCategory) {
		this.subItems = [];
	}
}
