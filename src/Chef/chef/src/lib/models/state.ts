import type { RecipeModel } from './viewmodels/recipeModel';

export class State {
	constructor(public recipes: RecipeModel[] | null, public fromCache: boolean) {}
}
