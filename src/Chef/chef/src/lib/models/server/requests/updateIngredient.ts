import type { IngredientNutritionData } from './ingredientNutritionData';
import type { IngredientPriceData } from './ingredientPriceData';

export class UpdateIngredient {
	constructor(
		public readonly id: number,
		public readonly taskId: number | null,
		public readonly name: string,
		public readonly nutritionData: IngredientNutritionData,
		public readonly priceData: IngredientPriceData
	) {}
}
