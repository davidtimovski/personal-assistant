import { inject, computedFrom } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ListsService } from "services/listsService";
import { List } from "models/entities/list";
import { ArchivedList } from "models/viewmodels/archivedList";
import { State } from "utils/state/state";

@inject(EventAggregator)
@connectTo()
export class ArchivedLists {
  private archivedLists: Array<ArchivedList>;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  state: State;

  constructor(private readonly eventAggregator: EventAggregator) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setListsFromState();
    });
  }

  async activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    if (!this.state.loading) {
      this.setListsFromState();
    }
  }

  setListsFromState() {
    this.archivedLists = this.state.lists
      .filter((x) => x.isArchived)
      .sort((a: List, b: List) => {
        const aDate = new Date(a.modifiedDate);
        const bDate = new Date(b.modifiedDate);
        if (aDate > bDate) return -1;
        if (aDate < bDate) return 1;
        return 0;
      });
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
