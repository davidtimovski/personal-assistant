import { ReviewIngredient } from "./reviewIngredient";
import { IngredientReviewSuggestion } from "./ingredientReviewSuggestion";

export class ReviewIngredientsModel {
  constructor(
    public id: number,
    public name: string,
    public description: string,
    public imageUri: string,
    public ingredients: Array<ReviewIngredient>,
    public ingredientSuggestions: Array<IngredientReviewSuggestion>
  ) {}
}
