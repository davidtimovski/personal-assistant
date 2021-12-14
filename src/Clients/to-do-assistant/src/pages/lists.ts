import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";
import { connectTo } from "aurelia-store";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { ProgressBar } from "../../../shared/src/models/progressBar";

import { ListsService } from "services/listsService";
import { UsersService } from "services/usersService";
import { LocalStorage } from "utils/localStorage";
import { List } from "models/entities/list";
import { ListModel } from "models/viewmodels/listModel";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import * as environment from "../../config/environment.json";
import { AppEvents } from "models/appEvents";

@inject(Router, ListsService, UsersService, LocalStorage, EventAggregator, I18N, ConnectionTracker)
@connectTo()
export class Lists {
  private imageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private progressBar = new ProgressBar();
  private computedLists: ListModel[];
  private lists: ListModel[];
  private listsLoaded = false;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  private isReordering = false;
  private menuButtonIsLoading = false;
  private computedListNameLookup;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly usersService: UsersService,
    private readonly localStorage: LocalStorage,
    private readonly eventAggregator: EventAggregator,
    private readonly i18n: I18N,
    private readonly connTracker: ConnectionTracker
  ) {
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
      this.setListsFromState();
      this.listsLoaded = true;
      this.progressBar.finish();
    });

    this.computedListNameLookup = {
      "high-priority": this.i18n.tr("highPriority"),
    };
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.imageUri = this.localStorage.getProfileImageUri();
  }

  attached() {
    if (this.state.lists) {
      this.setListsFromState();
      this.listsLoaded = true;
    } else {
      this.progressBar.start();
    }

    if (this.localStorage.isStale("profileImageUri")) {
      this.usersService.getProfileImageUri().then((imageUri) => {
        if (this.imageUri !== imageUri) {
          this.imageUri = imageUri;
        }
      });
    }

    this.eventAggregator.subscribe(AppEvents.TaskCompletedChangedRemotely, (data: any) => {
      this.setComputedListsFromState();
      this.setTaskCompletion(data.listId, data.isCompleted);
    });

    this.eventAggregator.subscribe(AppEvents.TaskDeletedRemotely, (data: any) => {
      this.setComputedListsFromState();
      this.setTaskCompletion(data.listId, true);
    });
  }

  setListsFromState() {
    this.setComputedListsFromState();

    this.lists = this.state.lists
      .filter((x) => !x.isArchived && !x.computedListType)
      .sort((a: List, b: List) => {
        return a.order - b.order;
      })
      .map(
        (x) =>
          new ListModel(
            x.id,
            x.name,
            x.icon,
            x.sharingState,
            x.order,
            x.computedListType,
            x.tasks.filter((x) => !x.isCompleted).length
          )
      );
  }

  setComputedListsFromState() {
    this.computedLists = this.state.lists
      .filter((x) => x.computedListType)
      .map(
        (x) =>
          new ListModel(
            x.id,
            this.computedListNameLookup[x.computedListType],
            x.icon,
            x.sharingState,
            x.order,
            x.computedListType,
            x.tasks.filter((x) => !x.isCompleted).length
          )
      );
  }

  setTaskCompletion(listId: number, isCompleted: boolean) {
    const list = this.lists.find((x) => x.id === listId);

    if (isCompleted) {
      list.uncompletedTaskCount--;
    } else {
      list.uncompletedTaskCount++;
    }
  }

  getComputedListIconClass(computedListType: string): string {
    return ListsService.getComputedListIconClass(computedListType);
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  async reorder(changedArray: List[], data: any) {
    const id: number = changedArray[data.toIndex].id;

    const oldOrder = ++data.fromIndex;
    const newOrder = ++data.toIndex;

    await this.listsService.reorder(id, oldOrder, newOrder);

    await Actions.reorderList(id, oldOrder, newOrder);

    this.isReordering = false;
  }

  sync() {
    this.progressBar.start();

    Actions.getLists(this.listsService).then(() => {
      this.setListsFromState();
      this.listsLoaded = true;
      this.progressBar.finish();
    });

    this.usersService.getProfileImageUri().then((imageUri) => {
      if (this.imageUri !== imageUri) {
        this.imageUri = imageUri;
      }
    });
  }

  @computedFrom("progressBar.progress")
  get getProgress() {
    if (this.progressBar.progress === 100) {
      this.progressBar.visible = false;
    }
    return this.progressBar.progress;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }

  goToMenu() {
    this.menuButtonIsLoading = true;
    this.router.navigateToRoute("menu");
  }

  detached() {
    window.clearInterval(this.progressBar.intervalId);
  }
}
