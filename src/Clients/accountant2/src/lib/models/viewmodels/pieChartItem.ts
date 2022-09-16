import type { AmountByCategory } from './amountByCategory';

export class PieChartItem {
	categoryId: number | null;
	parentCategoryId: number | null;
	categoryName: string | null;
	amount: number;
	color: string | null = null;
	subItems: PieChartItem[];

	constructor(data: AmountByCategory) {
		this.categoryId = data.categoryId;
		this.parentCategoryId = data.parentCategoryId;
		this.categoryName = data.categoryName;
		this.amount = data.amount;
		this.subItems = data.subItems.map((x) => new PieChartItem(x));
	}
}
