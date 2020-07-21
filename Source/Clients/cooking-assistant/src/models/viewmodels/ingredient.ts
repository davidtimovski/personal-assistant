export class Ingredient {
  constructor(
    public id: number,
    public recipeId: number,
    public taskId: number,
    public name: string,
    public amount: number,
    public unit: string,
    public missing: boolean,
    public nutritionSource: boolean,
    public costSource: boolean
  ) {}
}
