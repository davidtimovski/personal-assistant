import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { IngredientsService } from "services/ingredientsService";
import { SimpleIngredient } from "models/viewmodels/simpleIngredient";
import { I18N } from "aurelia-i18n";

@inject(Router, IngredientsService, I18N)
export class Ingredients {
  private ingredients: Array<SimpleIngredient>;
  private shadowIngredients: Array<SimpleIngredient>;
  private lastEditedId: number;
  private search = "";
  private clearSearchButtonIsVisible = false;
  private emptyListMessage: string;

  constructor(
    private readonly router: Router,
    private readonly ingredientsService: IngredientsService,
    private readonly i18n: I18N
  ) {}

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    this.ingredientsService
      .getAll()
      .then((ingredients: Array<SimpleIngredient>) => {
        this.ingredients = ingredients;
        this.shadowIngredients = ingredients.slice();

        this.emptyListMessage = this.i18n.tr("ingredients.emptyListMessage");
      });
  }

  searchInputChanged() {
    if (this.search.trim().length > 0) {
      this.clearSearchButtonIsVisible = true;
      this.ingredients = this.shadowIngredients.filter(
        (ingredient: SimpleIngredient) => {
          return ingredient.name
            .toUpperCase()
            .includes(this.search.trim().toUpperCase());
        }
      );
    } else {
      this.clearSearchButtonIsVisible = false;
      this.ingredients = this.shadowIngredients.slice();
    }
  }

  clearSearch() {
    this.search = "";
    this.ingredients = this.shadowIngredients.slice();
    this.clearSearchButtonIsVisible = false;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
