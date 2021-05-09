import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { List } from "models/entities/list";
import { ListsService } from "services/listsService";
import { SharingState } from "models/viewmodels/sharingState";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";

@inject(Router, ListsService, I18N, EventAggregator)
@connectTo()
export class ArchiveList {
  private listId: number;
  private model = new List(0, "", "", false, false, SharingState.NotShared, 0, false, [], null);
  private archiveButtonIsLoading = false;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setModelFromState();
    });
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
    if (list === null) {
      this.router.navigateToRoute("notFound");
    }

    this.model = JSON.parse(JSON.stringify(list));
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

      await Actions.getLists(this.listsService);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "archiveList.archiveSuccessful");

      this.router.navigateToRoute("lists");
    } catch {
      this.archiveButtonIsLoading = false;
    }
  }
}
