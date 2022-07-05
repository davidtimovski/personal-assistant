export class IngredientSuggestion {
  id: number;
  taskId: number;
  brandName: string;
  name: string;
  unit: string;
  unitImperial: string;
  selected: boolean;
  matched: boolean;
  hasNutritionData: boolean;
  hasPriceData: boolean;
  isPublic: boolean;
  children = new Array<IngredientSuggestion>();
}

export class IngredientCategory {
  name: string;
  ingredients: IngredientSuggestion[];
  subcategories: IngredientCategory[];
  matched: boolean;
}

export class PublicIngredientSuggestions {
  uncategorized: IngredientSuggestion[];
  categories: IngredientCategory[];
}

export class IngredientSuggestions {
  constructor(public userIngredients: IngredientSuggestion[], public publicIngredients: PublicIngredientSuggestions) {}
}

export enum IngredientPickerEvents {
  Added = "ingredient-picker:added",
  Selected = "ingredient-picker:selected",
  Unselect = "ingredient-picker:unselect",
  Reset = "ingredient-picker:reset",
}
