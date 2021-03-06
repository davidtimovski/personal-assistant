import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";

import { RecipesService } from "services/recipesService";
import { ShareRequest } from "models/viewmodels/shareRequest";
import * as Actions from "utils/state/actions";

@inject(Router, RecipesService, I18N)
export class ShareRequests {
  private pendingShareRequests: Array<ShareRequest>;
  private declinedShareRequests: Array<ShareRequest>;
  private shareRequestsTooltipKey = "shareRequests";
  private emptyListMessage: string;

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly i18n: I18N
  ) {}

  attached() {
    this.recipesService
      .getShareRequests()
      .then((allShareRequests: Array<ShareRequest>) => {
        this.pendingShareRequests = allShareRequests.filter(
          (request: ShareRequest) => {
            return request.isAccepted === null;
          }
        );
        this.declinedShareRequests = allShareRequests.filter(
          (request: ShareRequest) => {
            return request.isAccepted === false;
          }
        );

        this.emptyListMessage = this.i18n.tr("shareRequests.emptyListMessage");
      });
  }

  async accept(request: ShareRequest) {
    request.rightSideIsLoading = true;
    await this.recipesService.setShareIsAccepted(request.recipeId, true);
    this.pendingShareRequests.splice(
      this.pendingShareRequests.indexOf(request),
      1
    );

    await Actions.getRecipes(this.recipesService);

    this.router.navigateToRoute("recipesEdited", { editedId: request.recipeId });
  }

  async decline(request: ShareRequest) {
    request.leftSideIsLoading = true;
    await this.recipesService.setShareIsAccepted(request.recipeId, false);
    this.pendingShareRequests.splice(
      this.pendingShareRequests.indexOf(request),
      1
    );
    request.leftSideIsLoading = false;
    this.declinedShareRequests.unshift(request);
  }

  async delete(request: ShareRequest) {
    request.rightSideIsLoading = true;
    await this.recipesService.leave(request.recipeId);
    this.declinedShareRequests.splice(
      this.declinedShareRequests.indexOf(request),
      1
    );
    request.rightSideIsLoading = false;
  }
}
