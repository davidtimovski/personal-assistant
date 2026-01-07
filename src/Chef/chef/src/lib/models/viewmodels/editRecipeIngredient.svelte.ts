export class EditRecipeIngredient {
	amount = $state('');

	constructor(
		public id: number,
		public name: string,
		public parentName: string | null,
		amount: string,
		public unit: string | null,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public isNew: boolean
	) {
		this.amount = amount;
	}
}
