export class EditRecipeIngredient {
	constructor(
		public id: number,
		public name: string,
		public amount: string,
		public unit: string | null,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public isNew: boolean
	) {}
}
