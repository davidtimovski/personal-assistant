import { IngredientSuggestion } from "./ingredientSuggestions";

export class EditRecipeIngredient {
  constructor(
    public id: number,
    public taskId: number,
    public name: string,
    public amount: string,
    public unit: string,
    public existing: boolean,
    public suggestion: IngredientSuggestion
  ) {}
}
