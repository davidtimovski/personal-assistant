import { bindable, bindingMode, inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";

import { IngredientAutocompleteEvents, IngredientSuggestion } from "models/viewmodels/ingredientSuggestions";

@inject(EventAggregator)
export class PublicIngredientSuggestion {
  @bindable({ defaultBindingMode: bindingMode.toView }) ingredient: IngredientSuggestion;

  constructor(private readonly eventAggregator: EventAggregator) { }

  ingredientClicked() {
    this.eventAggregator.publish(IngredientAutocompleteEvents.Selected, this.ingredient);
    this.ingredient.selected = true;
  }
}
