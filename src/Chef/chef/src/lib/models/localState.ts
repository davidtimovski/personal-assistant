import type { RecipeModel } from './viewmodels/recipeModel';

export class LocalState {
	constructor(
		public recipes: RecipeModel[] | null,
		public fromCache: boolean
	) {}
}
