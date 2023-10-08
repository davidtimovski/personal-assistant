export class ShareRecipe {
	constructor(
		public readonly recipeId: number,
		public readonly newShares: number[],
		public readonly removeShares: number[]
	) {}
}
