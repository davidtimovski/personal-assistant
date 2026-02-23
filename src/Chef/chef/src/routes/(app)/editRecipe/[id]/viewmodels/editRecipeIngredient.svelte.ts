export class EditRecipeIngredient {
	amount = $state('');
	unit: string | null = $state(null);

	constructor(
		public id: number,
		public name: string,
		public parentName: string | null,
		amount: string,
		unit: string | null,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public isNew: boolean
	) {
		this.amount = amount;
		this.unit = unit;
	}
}
