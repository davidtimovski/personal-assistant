export class RecipeModel {
  constructor(
    public id: number,
    public name: string,
    public imageUri: string,
    public ingredientsMissing: number,
    public ingredientsMissingLabel: string,
    public lastOpenedDate: Date
  ) {}
}
