import { inject, bindable, bindingMode } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { ViewRecipe } from "../../models/viewmodels/viewRecipe";

@inject(I18N)
export class ServingsSelectorCustomElement {
  @bindable recipe: ViewRecipe;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) viewing: boolean;

  constructor(private readonly i18n: I18N) {}

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
      const previousServings = this.recipe.servings;
      this.recipe.servings--;

      this.adjustIngredientAmountToServings(
        previousServings,
        this.recipe.servings
      );
      this.adjustCostToServings(this.recipe.servings);
    }
  }

  incrementServings() {
    if (this.recipe.servings < 50) {
      const previousServings = this.recipe.servings;
      this.recipe.servings++;

      this.adjustIngredientAmountToServings(
        previousServings,
        this.recipe.servings
      );
      this.adjustCostToServings(this.recipe.servings);
    }
  }

  adjustIngredientAmountToServings(servings: number, newServings: number) {
    if (this.viewing) {
      for (let ingredient of this.recipe.ingredients) {
        if (ingredient.amount) {
          const amountPerServing = ingredient.amount / servings;
          ingredient.amount = parseFloat(
            (amountPerServing * newServings).toFixed(2)
          );
        }
      }
    }
  }

  adjustCostToServings(newServings: number) {
    if (this.viewing && this.recipe.costSummary.cost) {
      this.recipe.costSummary.cost = parseFloat(
        (this.recipe.costSummary.costPerServing * newServings).toFixed(2)
      );
    }
  }
}
