export class IngredientReplacement {
	constructor(
		public readonly id: number,
		public readonly replacementId: number,
		public readonly transferNutritionData: boolean,
		public readonly transferPriceData: boolean
	) {}
}
