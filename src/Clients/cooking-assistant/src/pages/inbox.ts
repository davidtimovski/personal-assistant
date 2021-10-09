import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { RecipesService } from "services/recipesService";
import { ReceivedRecipe } from "models/viewmodels/receivedRecipe";

@inject(Router, RecipesService)
export class Inbox {
  private pendingReceivedRecipes: Array<ReceivedRecipe>;
  private declinedReceivedRecipes: Array<ReceivedRecipe>;

  constructor(private readonly router: Router, private readonly recipesService: RecipesService) {}

  async activate() {
    const allReceivedRecipes = await this.recipesService.getSendRequests();
    this.pendingReceivedRecipes = allReceivedRecipes.filter((receivedRecipe: ReceivedRecipe) => {
      return !receivedRecipe.isDeclined;
    });
    this.declinedReceivedRecipes = allReceivedRecipes.filter((receivedRecipe: ReceivedRecipe) => {
      return receivedRecipe.isDeclined;
    });
  }

  async accept(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.rightSideIsLoading = true;

    const recipeId = await this.recipesService.tryImport(receivedRecipe.recipeId, [], true);
    if (recipeId > 0) {
      this.router.navigateToRoute("recipesEdited", {
        editedId: recipeId,
      });
    } else {
      this.router.navigateToRoute("reviewIngredients", {
        id: receivedRecipe.recipeId,
      });
    }
  }

  async decline(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.leftSideIsLoading = true;
    await this.recipesService.declineSendRequest(receivedRecipe.recipeId);
    this.pendingReceivedRecipes.splice(this.pendingReceivedRecipes.indexOf(receivedRecipe), 1);
    receivedRecipe.leftSideIsLoading = false;
    this.declinedReceivedRecipes.unshift(receivedRecipe);
  }

  async delete(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.rightSideIsLoading = true;
    await this.recipesService.deleteSendRequest(receivedRecipe.recipeId);
    this.declinedReceivedRecipes.splice(this.declinedReceivedRecipes.indexOf(receivedRecipe), 1);
    receivedRecipe.rightSideIsLoading = false;
  }
}
