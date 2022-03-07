export class Ingredient {
  id: number;
  recipeId: number;
  name: string;
  amount: number;
  amountPerServing: number;
  unit: string;
  missing: boolean;
  nutritionSource: boolean;
  costSource: boolean;
}
