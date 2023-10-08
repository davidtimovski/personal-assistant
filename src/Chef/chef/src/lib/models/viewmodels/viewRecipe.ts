import type { Ingredient } from './ingredient';
import type { NutritionSummary } from './nutritionSummary';
import type { CostSummary } from './costSummary';
import type { SharingState } from './sharingState';

export class ViewRecipe {
	constructor(
		public id: number,
		public name: string,
		public description: string,
		public ingredients: Array<Ingredient>,
		public instructions: string,
		public prepDuration: string,
		public cookDuration: string,
		public servings: number,
		public imageUri: string,
		public videoUrl: string,
		public lastOpenedDate: Date,
		public nutritionSummary: NutritionSummary,
		public costSummary: CostSummary,
		public sharingState: SharingState,
		public imageWidth: number,
		public imageHeight: number
	) {
		this.ingredients = ingredients || new Array<Ingredient>();
	}
}
