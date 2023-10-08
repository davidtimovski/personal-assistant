export class CreateSendRequest {
	constructor(public readonly recipeId: number, public readonly recipientsIds: number[]) {}
}
