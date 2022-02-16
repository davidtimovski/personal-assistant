export class IngredientSuggestion {
  selected: boolean;
  matched: boolean;
  children = new Array<IngredientSuggestion>();

  constructor(
    public id: number,
    public taskId: number,
    public name: string,
    public unit: string
  ) {}
}

export class IngredientCategory {
  name: string;
  ingredients: Array<IngredientSuggestion>;
  subcategories: Array<IngredientCategory>;
  matched: boolean;
}

export class PublicIngredientSuggestions {
  uncategorized: Array<IngredientSuggestion>;
  categories: Array<IngredientCategory>;
}

export class IngredientSuggestions {
  userIngredients: Array<IngredientSuggestion>;
  publicIngredients: PublicIngredientSuggestions;
}

export enum IngredientAutocompleteEvents {
  Selected = "ingredient-autocomplete:selected"
}
