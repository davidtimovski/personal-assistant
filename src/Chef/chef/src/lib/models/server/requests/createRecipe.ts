import type { UpdateRecipeIngredient } from './updateRecipeIngredient';

export class CreateRecipe {
	constructor(
		public readonly name: string,
		public readonly description: string | null,
		public readonly ingredients: UpdateRecipeIngredient[],
		public readonly instructions: string | null,
		public readonly prepDuration: string | null,
		public readonly cookDuration: string | null,
		public readonly servings: number,
		public readonly imageUri: string | null,
		public readonly videoUrl: string | null
	) {}
}
