import { IngredientSuggestion } from "./ingredientSuggestion";

export class IngredientSuggestions {
  constructor(
    public suggestions: Array<IngredientSuggestion>,
    public taskSuggestions: Array<IngredientSuggestion>
  ) {}
}
