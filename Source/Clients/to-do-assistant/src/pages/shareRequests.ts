import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ListsService } from "services/listsService";
import { LocalStorage } from "utils/localStorage";
import { ShareRequest } from "models/viewmodels/shareRequest";
import { I18N } from "aurelia-i18n";

@inject(Router, ListsService, LocalStorage, I18N)
export class ShareRequests {
  private pendingShareRequests: Array<ShareRequest>;
  private declinedShareRequests: Array<ShareRequest>;
  private shareRequestsTooltipKey = "shareRequests";
  private emptyListMessage: string;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {}

  attached() {
    this.listsService
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
    await this.listsService.setShareIsAccepted(request.listId, true);
    this.pendingShareRequests.splice(
      this.pendingShareRequests.indexOf(request),
      1
    );

    this.localStorage.setDataLastLoad(new Date(0));

    this.router.navigateToRoute("listsEdited", { editedId: request.listId });
  }

  async decline(request: ShareRequest) {
    request.leftSideIsLoading = true;
    await this.listsService.setShareIsAccepted(request.listId, false);
    this.pendingShareRequests.splice(
      this.pendingShareRequests.indexOf(request),
      1
    );
    request.leftSideIsLoading = false;
    this.declinedShareRequests.unshift(request);
  }

  async delete(request: ShareRequest) {
    request.rightSideIsLoading = true;
    await this.listsService.leave(request.listId);
    this.declinedShareRequests.splice(
      this.declinedShareRequests.indexOf(request),
      1
    );
    request.rightSideIsLoading = false;
  }
}
