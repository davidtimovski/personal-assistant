export class CanSendRecipe {
  constructor(
    public userId: number,
    public imageUri: string,
    public canSend: boolean,
    public alreadySent: boolean
  ) {}
}
