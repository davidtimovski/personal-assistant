import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";

import { RecipesService } from "services/recipesService";
import { ReviewIngredientsModel } from "models/viewmodels/reviewIngredientsModel";
import { ReviewIngredient } from "models/viewmodels/reviewIngredient";
import { IngredientReplacement } from "models/viewmodels/ingredientReplacement";
import { IngredientSuggestion } from "models/viewmodels/ingredientSuggestions";
import * as Actions from "utils/state/actions";

@inject(Router, RecipesService, I18N)
export class ReviewIngredients {
  private model: ReviewIngredientsModel;
  private reviewing = false;
  private introductoryLabel: string;
  private currentIngredient: ReviewIngredient;
  private pickExistingIngredientInput: HTMLInputElement;
  private doneButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly i18n: I18N
  ) {}

  async activate(params: any) {
    this.model = await this.recipesService.getForReview(params.id);

    this.introductoryLabel =
      this.model.ingredients.length === 1
        ? this.i18n.tr("reviewIngredients.introductoryLabel")
        : this.i18n.tr("reviewIngredients.introductoryLabelPlural", {
            ingredients: this.model.ingredients.length,
          });
    this.currentIngredient = this.model.ingredients[0];
  }

  attached() {}

  @computedFrom("currentIngredient")
  get currentIngredientNumber(): number {
    return this.model.ingredients.indexOf(this.currentIngredient) + 1;
  }

  startReviewing() {
    this.reviewing = true;
  }

  setReplacement(suggestion: IngredientSuggestion) {
    this.currentIngredient.replacementId = suggestion.id;
    this.currentIngredient.replacementName = suggestion.name;
    this.currentIngredient.transferNutritionData =
      this.currentIngredient.hasNutritionData && !suggestion.hasNutritionData;
    this.currentIngredient.transferPriceData = this.currentIngredient.hasPriceData && !suggestion.hasPriceData;
  }

  revertReplacement() {
    this.currentIngredient.replacementId = this.currentIngredient.replacementName = null;
  }

  previousIngredient() {
    let currentIndex = this.model.ingredients.indexOf(this.currentIngredient);
    if (currentIndex > 0) {
      this.currentIngredient = this.model.ingredients[--currentIndex];
    } else {
      this.reviewing = false;
    }
  }

  nextIngredient() {
    let currentIndex = this.model.ingredients.indexOf(this.currentIngredient);
    if (currentIndex < this.model.ingredients.length - 1) {
      this.currentIngredient = this.model.ingredients[++currentIndex];
    }
  }

  async import() {
    if (this.doneButtonIsLoading) {
      return;
    }

    this.doneButtonIsLoading = true;

    const ingredientReplacements = this.model.ingredients
      .filter((ingredient: ReviewIngredient) => {
        return !!ingredient.replacementId;
      })
      .map((ingredient: ReviewIngredient) => {
        return new IngredientReplacement(
          ingredient.id,
          ingredient.replacementId,
          ingredient.transferNutritionData,
          ingredient.transferPriceData
        );
      });

    const recipeId = await this.recipesService.tryImport(this.model.id, ingredientReplacements, false);

    await Actions.getRecipes(this.recipesService);

    this.router.navigateToRoute("recipesEdited", {
      editedId: recipeId,
    });
  }
}
