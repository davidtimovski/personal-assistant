import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { RecipesService } from "services/recipesService";
import { I18N } from "aurelia-i18n";
import { ReviewIngredientsModel } from "models/viewmodels/reviewIngredientsModel";
import { ReviewIngredient } from "models/viewmodels/reviewIngredient";
import { IngredientReviewSuggestion } from "models/viewmodels/IngredientReviewSuggestion";
import { IngredientReplacement } from "models/viewmodels/ingredientReplacement";
import autocomplete from "autocompleter";
import * as Actions from "utils/state/actions";

@inject(Router, RecipesService, I18N)
export class ReviewIngredients {
  private model: ReviewIngredientsModel;
  private reviewing = false;
  private introductoryLabel: string;
  private currentIngredient: ReviewIngredient;
  private pickExistingIngredientInput: HTMLInputElement;
  private currentSuggestion: IngredientReviewSuggestion;
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

    this.findCurrentSuggestion();
  }

  attached() {
    this.attachAutocomplete(this.model.ingredientSuggestions);
  }

  attachAutocomplete(ingredientSuggestions: Array<IngredientReviewSuggestion>) {
    autocomplete({
      input: this.pickExistingIngredientInput,
      minLength: 1,
      fetch: (text: string, update: (items: IngredientReviewSuggestion[]) => void) => {
        const suggestions = ingredientSuggestions.filter((i) => i.name.toUpperCase().startsWith(text.toUpperCase()));
        update(suggestions);
      },
      onSelect: (suggestion: IngredientReviewSuggestion) => {
        this.setReplacement(suggestion);

        this.pickExistingIngredientInput.value = "";
      },
      className: "autocomplete-customizations",
    });
  }

  @computedFrom("currentIngredient")
  get currentIngredientNumber(): number {
    return this.model.ingredients.indexOf(this.currentIngredient) + 1;
  }

  startReviewing() {
    this.reviewing = true;
  }

  findCurrentSuggestion() {
    if (!this.currentIngredient.replacementId) {
      this.currentSuggestion = this.model.ingredientSuggestions.find((suggestion: IngredientReviewSuggestion) => {
        return suggestion.name.toUpperCase() === this.currentIngredient.name.toUpperCase();
      });
    }
  }

  setReplacement(suggestion: IngredientReviewSuggestion) {
    this.currentIngredient.replacementId = suggestion.id;
    this.currentIngredient.replacementName = suggestion.name;
    this.currentIngredient.transferNutritionData =
      this.currentIngredient.hasNutritionData && !suggestion.hasNutritionData;
    this.currentIngredient.transferPriceData = this.currentIngredient.hasPriceData && !suggestion.hasPriceData;
    this.currentSuggestion = undefined;
  }

  revertReplacement() {
    this.currentIngredient.replacementId = this.currentIngredient.replacementName = null;
    this.findCurrentSuggestion();
  }

  previousIngredient() {
    let currentIndex = this.model.ingredients.indexOf(this.currentIngredient);
    if (currentIndex > 0) {
      this.currentIngredient = this.model.ingredients[--currentIndex];
    } else {
      this.reviewing = false;
    }
    this.findCurrentSuggestion();
  }

  nextIngredient() {
    let currentIndex = this.model.ingredients.indexOf(this.currentIngredient);
    if (currentIndex < this.model.ingredients.length - 1) {
      this.currentIngredient = this.model.ingredients[++currentIndex];
    }
    this.findCurrentSuggestion();
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
