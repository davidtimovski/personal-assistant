import type { IngredientReplacement } from './ingredientReplacement';

export class ImportRecipe {
	constructor(
		public readonly id: number,
		public readonly ingredientReplacements: IngredientReplacement[],
		public readonly checkIfReviewRequired: boolean
	) {}
}
