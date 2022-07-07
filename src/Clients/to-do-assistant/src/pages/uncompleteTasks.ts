import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { List } from "models/entities";
import { ListsService } from "services/listsService";
import { State } from "utils/state/state";
import { SharingState } from "models/viewmodels/sharingState";
import { AppEvents } from "models/appEvents";

@autoinject
@connectTo()
export class UncompleteTasks {
  private listId: number;
  private model = new List(0, "", "", false, false, SharingState.NotShared, 0, false, null, [], null);
  private uncompleteText: string;
  private uncompleteButtonIsLoading = false;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
      this.setModelFromState();
    });

    this.uncompleteText = this.i18n.tr("uncompleteTasks.thisWillUncomplete");
  }

  activate(params: any) {
    this.listId = parseInt(params.id, 10);
  }

  attached() {
    if (!this.state.loading) {
      this.setModelFromState();
    }
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.id === this.listId);
    if (!list) {
      this.router.navigateToRoute("notFound");
    } else {
      this.model = JSON.parse(JSON.stringify(list));

      if (this.model.sharingState !== 0) {
        this.uncompleteText = this.i18n.tr("uncompleteTasks.thisWillUncompleteShared");
      }
    }
  }

  async uncomplete() {
    if (this.uncompleteButtonIsLoading) {
      return;
    }

    this.uncompleteButtonIsLoading = true;

    try {
      await this.listsService.setTasksAsNotCompleted(this.model.id);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "uncompleteTasks.uncompleteTasksSuccessful");

      this.router.navigateToRoute("listsEdited", { editedId: this.model.id });
    } catch {
      this.uncompleteButtonIsLoading = false;
    }
  }
}
