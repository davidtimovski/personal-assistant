export class AmountByCategory {
	subItems: AmountByCategory[] = [];

	constructor(
		public categoryId: number | null,
		public parentCategoryId: number | null,
		public categoryName: string | null,
		public amount: number,
		public totalAmount: number
	) {}
}
