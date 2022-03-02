export class IngredientSuggestion {
  selected: boolean;
  matched: boolean;
  hasNutritionData: boolean;
  hasPriceData: boolean;
  children = new Array<IngredientSuggestion>();

  constructor(public id: number, public taskId: number, public name: string, public unit: string) {}
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
  constructor(
    public userIngredients: Array<IngredientSuggestion>,
    public publicIngredients: PublicIngredientSuggestions
  ) {}
}

export enum IngredientPickerEvents {
  Added = "ingredient-picker:added",
  Selected = "ingredient-picker:selected",
  Unselect = "ingredient-picker:unselect",
  Reset = "ingredient-picker:reset",
}
