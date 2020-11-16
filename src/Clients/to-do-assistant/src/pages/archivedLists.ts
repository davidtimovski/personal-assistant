import { inject, computedFrom } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";
import { connectTo } from "aurelia-store";

import { ListsService } from "services/listsService";
import { List } from "models/entities/list";
import { ArchivedList } from "models/viewmodels/archivedList";
import { State } from "utils/state/state";

@inject(EventAggregator, I18N)
@connectTo()
export class ArchivedLists {
  private archivedLists: Array<ArchivedList>;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  private emptyListMessage: string;
  state: State;

  constructor(
    private readonly eventAggregator: EventAggregator,
    private readonly i18n: I18N
  ) {
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

    this.emptyListMessage = this.i18n.tr("archivedLists.emptyListMessage");
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
