export class CanShareRecipe {
  constructor(
    public userId: number,
    public imageUri: string,
    public canShare: boolean
  ) {}
}
