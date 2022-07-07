import { autoinject, computedFrom } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ListsService } from "services/listsService";
import { ArchivedList } from "models/viewmodels/archivedList";
import { State } from "utils/state/state";
import { AppEvents } from "models/appEvents";

@autoinject
@connectTo()
export class ArchivedLists {
  private archivedLists: Array<ArchivedList>;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  state: State;

  constructor(private readonly eventAggregator: EventAggregator) {
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
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
    this.archivedLists = ListsService.getArchived(this.state.lists);
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
