import { inject, computedFrom } from "aurelia-framework";
import { IngredientsService } from "services/ingredientsService";
import { SimpleIngredient } from "models/viewmodels/simpleIngredient";

@inject(IngredientsService)
export class Ingredients {
  private ingredients: Array<SimpleIngredient>;
  private shadowIngredients: Array<SimpleIngredient>;
  private lastEditedId: number;
  private search = "";
  private clearSearchButtonIsVisible = false;

  constructor(private readonly ingredientsService: IngredientsService) {}

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    this.ingredientsService.getAll().then((ingredients: Array<SimpleIngredient>) => {
      this.ingredients = ingredients;
      this.shadowIngredients = ingredients.slice();
    });
  }

  searchInputChanged() {
    if (this.search.trim().length > 0) {
      this.clearSearchButtonIsVisible = true;
      this.ingredients = this.shadowIngredients.filter((ingredient: SimpleIngredient) => {
        return ingredient.name.toUpperCase().includes(this.search.trim().toUpperCase());
      });
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
