export class CanShareRecipe {
	constructor(public canShare: boolean, public userId: number | null, public imageUri: string | null) {}
}
