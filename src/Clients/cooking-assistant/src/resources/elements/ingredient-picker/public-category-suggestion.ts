import { bindable, bindingMode } from "aurelia-framework";
import { IngredientCategory } from "../../../models/viewmodels/ingredientSuggestions";

export class PublicCategorySuggestion {
  @bindable({ defaultBindingMode: bindingMode.toView }) category: IngredientCategory;
}
