import { ReviewIngredient } from "./reviewIngredient";

export class ReviewIngredientsModel {
  id: number;
  name: string;
  description: string;
  imageUri: string;
  ingredients: Array<ReviewIngredient>;
}
