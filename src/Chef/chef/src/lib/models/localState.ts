import type { RecipeResponse } from './server/responses/recipeResponse';

export class LocalState {
	constructor(
		public recipes: RecipeResponse[] | null,
		public fromCache: boolean
	) {}
}
