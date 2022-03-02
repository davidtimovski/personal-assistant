import { bindable, bindingMode, inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../../../shared/src/utils/validationUtil";

import { IngredientsService } from "services/ingredientsService";
import {
  IngredientCategory,
  IngredientPickerEvents,
  IngredientSuggestion,
  IngredientSuggestions,
} from "models/viewmodels/ingredientSuggestions";

@inject(IngredientsService, EventAggregator)
export class IngredientPicker {
  private ingredientName = "";
  private ingredientNameIsInvalid: boolean;
  private suggestions: IngredientSuggestions;
  private suggestionsMatched = false;

  @bindable({ defaultBindingMode: bindingMode.toView }) addingEnabled: boolean;
  @bindable({ defaultBindingMode: bindingMode.toView }) recipeIngredientIds: number[];

  constructor(
    private readonly ingredientsService: IngredientsService,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe(IngredientPickerEvents.Selected, () => {
      this.ingredientName = "";
      this.ingredientNameIsInvalid = false;
      this.resetIngredientMatches();
    });

    this.eventAggregator.subscribe(IngredientPickerEvents.Unselect, (ingredientId: number) => {
      const suggestion = this.findSuggestion(ingredientId);
      suggestion.selected = false;
    });

    this.eventAggregator.subscribe(IngredientPickerEvents.Reset, () => {
      this.resetIngredientMatches();
    });
  }

  attached() {
    const userSuggestionsPromise = this.ingredientsService.getUserIngredientSuggestions();
    const publicSuggestionsPromise = this.ingredientsService.getPublicIngredientSuggestions();

    Promise.all([userSuggestionsPromise, publicSuggestionsPromise]).then((suggestions) => {
      this.suggestions = new IngredientSuggestions(suggestions[0], suggestions[1]);

      if (this.recipeIngredientIds) {
        for (const id of this.recipeIngredientIds) {
          const suggestion = this.findSuggestion(id);
          suggestion.selected = true;
        }
      }
    });
  }

  addNewIngredient() {
    this.ingredientNameIsInvalid = ValidationUtil.isEmptyOrWhitespace(this.ingredientName);

    if (!this.ingredientNameIsInvalid) {
      this.eventAggregator.publish(IngredientPickerEvents.Added, this.ingredientName);
      this.ingredientName = "";
    }

    this.resetIngredientMatches();
  }

  ingredientInputChanged() {
    this.resetIngredientMatches();

    const search = this.ingredientName.trim().toUpperCase();
    if (!search || search.length < 3) {
      return;
    }

    this.suggestions.userIngredients.forEach((x) => this.searchIngredient(x, search));
    this.suggestions.publicIngredients.uncategorized.forEach((x) => this.searchIngredient(x, search));
    this.suggestions.publicIngredients.categories.forEach((x) => this.searchCategory(x, search));
  }

  resetIngredientMatches() {
    this.suggestions.userIngredients.forEach((x) => this.matchIngredient(x, false));
    this.suggestions.publicIngredients.uncategorized.forEach((x) => this.matchIngredient(x, false));
    this.suggestions.publicIngredients.categories.forEach((x) => this.matchCategory(x, false));

    this.suggestionsMatched = false;
  }

  matchIngredient(ingredient: IngredientSuggestion, matched: boolean) {
    ingredient.matched = matched;
    ingredient.children.forEach((x) => this.matchIngredient(x, matched));
  }

  searchIngredient(ingredient: IngredientSuggestion, search: string) {
    ingredient.matched = ingredient.name.toUpperCase().includes(search);

    if (ingredient.matched) {
      this.matchIngredient(ingredient, true);
      this.suggestionsMatched = true;
    } else {
      for (const child of ingredient.children) {
        if (this.searchIngredient(child, search)) {
          ingredient.matched = true;
        }
      }
    }

    return ingredient.matched;
  }

  matchCategory(category: IngredientCategory, matched: boolean) {
    category.matched = matched;
    category.ingredients.forEach((x) => this.matchIngredient(x, matched));
    category.subcategories.forEach((x) => this.matchCategory(x, matched));
  }

  searchCategory(category: IngredientCategory, search: string) {
    category.matched = category.name.toUpperCase().includes(search);

    if (category.matched) {
      this.matchCategory(category, true);
      this.suggestionsMatched = true;
    } else {
      for (const ingredient of category.ingredients) {
        if (this.searchIngredient(ingredient, search)) {
          category.matched = true;
        }
      }

      for (const subcategory of category.subcategories) {
        if (this.searchCategory(subcategory, search)) {
          category.matched = true;
        }
      }
    }

    return category.matched;
  }

  findInIngredientRecursive(ingredient: IngredientSuggestion, ingredientId: number) {
    if (ingredient.id === ingredientId) {
      return ingredient;
    }

    let result = null;
    for (let i = 0; result == null && i < ingredient.children.length; i++) {
      result = this.findInIngredientRecursive(ingredient.children[i], ingredientId);
    }
    return result;
  }

  findSuggestion(ingredientId: number) {
    let foundSuggestion = this.suggestions.userIngredients.find((x) => x.id === ingredientId);
    if (foundSuggestion) {
      return foundSuggestion;
    }

    for (const suggestion of this.suggestions.publicIngredients.uncategorized) {
      foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
      if (foundSuggestion) {
        return foundSuggestion;
      }
    }

    for (const category of this.suggestions.publicIngredients.categories) {
      for (const suggestion of category.ingredients) {
        foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
        if (foundSuggestion) {
          return foundSuggestion;
        }
      }

      for (const subcategory of category.subcategories) {
        for (const suggestion of subcategory.ingredients) {
          foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
          if (foundSuggestion) {
            return foundSuggestion;
          }
        }
      }
    }

    throw "Could not find suggestion";
  }
}
