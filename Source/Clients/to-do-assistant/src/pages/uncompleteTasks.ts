import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { List } from "models/entities/list";
import { ListsService } from "services/listsService";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

@inject(Router, ListsService, I18N, EventAggregator)
export class UncompleteTasks {
  private model: List;
  private uncompleteText: string;
  private uncompleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.uncompleteText = this.i18n.tr("uncompleteTasks.thisWillUncomplete");
  }

  async activate(params: any) {
    this.model = await this.listsService.get(params.id);
    if (this.model === null) {
      this.router.navigateToRoute("notFound");
    }

    if (this.model.sharingState !== 0) {
      this.uncompleteText = this.i18n.tr(
        "uncompleteTasks.thisWillUncompleteShared"
      );
    }
  }

  async uncomplete() {
    if (this.uncompleteButtonIsLoading) {
      return;
    }

    this.uncompleteButtonIsLoading = true;

    try {
      await this.listsService.setTasksAsNotCompleted(this.model.id);

      this.eventAggregator.publish(
        "alert-success",
        "uncompleteTasks.uncompleteTasksSuccessful"
      );

      this.router.navigateToRoute("listsEdited", { editedId: this.model.id });
    } catch {
      this.uncompleteButtonIsLoading = false;
    }
  }
}
