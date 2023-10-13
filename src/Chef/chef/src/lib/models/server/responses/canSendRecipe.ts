export class CanSendRecipe {
	constructor(public canSend: boolean, public userId: number | null, public imageUri: string | null, public alreadySent: boolean | null) {}
}
