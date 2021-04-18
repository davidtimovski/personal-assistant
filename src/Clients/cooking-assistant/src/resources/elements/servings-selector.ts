import { inject, bindable, bindingMode } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { ViewRecipe } from "../../models/viewmodels/viewRecipe";

@inject(I18N)
export class ServingsSelectorCustomElement {
  @bindable recipe: ViewRecipe;
  @bindable({ defaultBindingMode: bindingMode.toView }) viewing: boolean;

  constructor(private readonly i18n: I18N) { }

  get servingsLabel() {
    if (this.recipe.servings > 1) {
      return this.i18n.tr("recipe.servings", {
        servings: this.recipe.servings
      });
    }

    return this.i18n.tr("recipe.oneServing");
  }

  decrementServings() {
    if (this.recipe.servings > 1) {
      this.recipe.servings--;

      this.adjustIngredientAmountToServings();
      this.adjustCostToServings();
    }
  }

  incrementServings() {
    if (this.recipe.servings < 50) {
      this.recipe.servings++;

      this.adjustIngredientAmountToServings();
      this.adjustCostToServings();
    }
  }

  adjustIngredientAmountToServings() {
    if (this.viewing) {
      for (let ingredient of this.recipe.ingredients.filter(x => x.amount)) {
        ingredient.amount = parseFloat(
          (ingredient.amountPerServing * this.recipe.servings).toFixed(2)
        );
      }
    }
  }

  adjustCostToServings() {
    if (this.viewing && this.recipe.costSummary.cost) {
      this.recipe.costSummary.cost = parseFloat(
        (this.recipe.costSummary.costPerServing * this.recipe.servings).toFixed(2)
      );
    }
  }
}
