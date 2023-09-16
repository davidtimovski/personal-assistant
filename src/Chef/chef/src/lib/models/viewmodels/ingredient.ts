export class Ingredient {
	constructor(
		public id: number,
		public recipeId: number,
		public name: string,
		public amount: number,
		public amountPerServing: number,
		public unit: string,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public missing: boolean
	) {}
}
