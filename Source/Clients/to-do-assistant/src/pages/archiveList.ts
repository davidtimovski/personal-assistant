import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { List } from "models/entities/list";
import { ListsService } from "services/listsService";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

@inject(Router, ListsService, I18N, EventAggregator)
export class ArchiveList {
  private model: List;
  private archiveButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {}

  async activate(params: any) {
    this.model = await this.listsService.get(params.id);
    if (this.model === null) {
      this.router.navigateToRoute("notFound");
    }
  }

  @computedFrom("model.sharingState")
  get archivingAListText() {
    return this.model.sharingState === 0
      ? this.i18n.tr("archiveList.archivingAList")
      : this.i18n.tr("archiveList.archivingAListShared");
  }

  async archive() {
    if (this.archiveButtonIsLoading) {
      return;
    }

    this.archiveButtonIsLoading = true;

    try {
      await this.listsService.setIsArchived(this.model.id, true);

      this.eventAggregator.publish(
        "alert-success",
        "archiveList.archiveSuccessful"
      );

      this.router.navigateToRoute("listsEdited", { editedId: this.model.id });
    } catch {
      this.archiveButtonIsLoading = false;
    }
  }
}
