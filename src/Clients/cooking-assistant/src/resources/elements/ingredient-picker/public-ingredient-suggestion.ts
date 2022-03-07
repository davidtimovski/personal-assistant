import { bindable, bindingMode, inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";

import { IngredientPickerEvents, IngredientSuggestion } from "../../../models/viewmodels/ingredientSuggestions";

@inject(EventAggregator)
export class PublicIngredientSuggestion {
  @bindable({ defaultBindingMode: bindingMode.toView }) ingredient: IngredientSuggestion;

  constructor(private readonly eventAggregator: EventAggregator) {}

  ingredientClicked() {
    if (this.ingredient.selected) {
      return;
    }

    this.eventAggregator.publish(IngredientPickerEvents.Selected, this.ingredient);
    this.ingredient.selected = true;
  }
}
