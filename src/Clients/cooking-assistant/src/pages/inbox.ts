import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { RecipesService } from "services/recipesService";
import { ReceivedRecipe } from "models/viewmodels/receivedRecipe";
import { I18N } from "aurelia-i18n";

@inject(Router, RecipesService, I18N)
export class Inbox {
  private pendingReceivedRecipes: Array<ReceivedRecipe>;
  private declinedReceivedRecipes: Array<ReceivedRecipe>;
  private receivedRecipesTooltipKey = "receivedRecipes";
  private emptyListMessage: string;

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly i18n: I18N
  ) {}

  async activate() {
    const allReceivedRecipes = await this.recipesService.getSendRequests();
    this.pendingReceivedRecipes = allReceivedRecipes.filter(
      (receivedRecipe: ReceivedRecipe) => {
        return !receivedRecipe.isDeclined;
      }
    );
    this.declinedReceivedRecipes = allReceivedRecipes.filter(
      (receivedRecipe: ReceivedRecipe) => {
        return receivedRecipe.isDeclined;
      }
    );

    this.emptyListMessage = this.i18n.tr("inbox.emptyListMessage");
  }

  async accept(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.rightSideIsLoading = true;

    const recipeId = await this.recipesService.tryImport(
      receivedRecipe.recipeId,
      [],
      true
    );
    if (recipeId > 0) {
      this.router.navigateToRoute("recipesEdited", {
        editedId: recipeId
      });
    } else {
      this.router.navigateToRoute("reviewIngredients", {
        id: receivedRecipe.recipeId
      });
    }
  }

  async decline(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.leftSideIsLoading = true;
    await this.recipesService.declineSendRequest(receivedRecipe.recipeId);
    this.pendingReceivedRecipes.splice(
      this.pendingReceivedRecipes.indexOf(receivedRecipe),
      1
    );
    receivedRecipe.leftSideIsLoading = false;
    this.declinedReceivedRecipes.unshift(receivedRecipe);
  }

  async delete(receivedRecipe: ReceivedRecipe) {
    receivedRecipe.rightSideIsLoading = true;
    await this.recipesService.deleteSendRequest(receivedRecipe.recipeId);
    this.declinedReceivedRecipes.splice(
      this.declinedReceivedRecipes.indexOf(receivedRecipe),
      1
    );
    receivedRecipe.rightSideIsLoading = false;
  }
}
