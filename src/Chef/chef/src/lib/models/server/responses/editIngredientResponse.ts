import type { NutritionData } from '../../nutritionData';
import type { PriceData } from '../../priceData';
import type { SimpleRecipe } from '../../viewmodels/simpleRecipe';

export class EditIngredientResponse {
	constructor(
		public id: number,
		public taskId: number,
		public taskName: string,
		public taskList: string,
		public name: string,
		public nutritionData: NutritionData,
		public priceData: PriceData,
		public recipes: SimpleRecipe[]
	) {}
}
