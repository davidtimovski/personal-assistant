export class Ingredient {
  id: number;
  recipeId: number;
  name: string;
  amount: number;
  amountPerServing: number;
  unit: string;
  hasNutritionData: boolean;
  hasPriceData: boolean;
  missing: boolean;
}
