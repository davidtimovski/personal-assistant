import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";

import { ListsService } from "services/listsService";
import { ShareRequest } from "models/viewmodels/shareRequest";

@inject(Router, ListsService)
export class ShareRequests {
  private pendingShareRequests: Array<ShareRequest>;
  private declinedShareRequests: Array<ShareRequest>;

  constructor(private readonly router: Router, private readonly listsService: ListsService) {}

  async attached() {
    const allShareRequests = await this.listsService.getShareRequests();

    this.pendingShareRequests = allShareRequests.filter((request: ShareRequest) => {
      return request.isAccepted === null;
    });
    this.declinedShareRequests = allShareRequests.filter((request: ShareRequest) => {
      return request.isAccepted === false;
    });
  }

  async accept(request: ShareRequest) {
    request.rightSideIsLoading = true;

    await this.listsService.setShareIsAccepted(request.listId, true);
    this.pendingShareRequests.splice(this.pendingShareRequests.indexOf(request), 1);

    this.router.navigateToRoute("listsEdited", { editedId: request.listId });
  }

  async decline(request: ShareRequest) {
    request.leftSideIsLoading = true;
    await this.listsService.setShareIsAccepted(request.listId, false);
    this.pendingShareRequests.splice(this.pendingShareRequests.indexOf(request), 1);
    request.leftSideIsLoading = false;
    this.declinedShareRequests.unshift(request);
  }

  async delete(request: ShareRequest) {
    request.rightSideIsLoading = true;
    await this.listsService.leave(request.listId);
    this.declinedShareRequests.splice(this.declinedShareRequests.indexOf(request), 1);
    request.rightSideIsLoading = false;
  }
}
