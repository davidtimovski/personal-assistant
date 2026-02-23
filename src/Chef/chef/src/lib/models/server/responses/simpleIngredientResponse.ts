export class SimpleIngredientResponse {
	constructor(
		public id: number,
		public taskId: number,
		public name: string,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public unused: boolean,
		public isPublic: boolean
	) {}
}
