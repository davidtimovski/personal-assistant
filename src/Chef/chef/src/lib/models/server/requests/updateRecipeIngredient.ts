export class UpdateRecipeIngredient {
	constructor(public readonly id: number | null, public readonly name: string, public readonly amount: number, public readonly unit: string | null) {}
}
