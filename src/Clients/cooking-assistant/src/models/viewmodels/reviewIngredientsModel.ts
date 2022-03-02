import { ReviewIngredient } from "./reviewIngredient";

export class ReviewIngredientsModel {
  constructor(
    public id: number,
    public name: string,
    public description: string,
    public imageUri: string,
    public ingredients: Array<ReviewIngredient>
  ) {}
}
