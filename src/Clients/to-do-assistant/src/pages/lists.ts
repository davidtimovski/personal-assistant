import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { ProgressBar } from "../../../shared/src/models/progressBar";

import { ListsService } from "services/listsService";
import { UsersService } from "services/usersService";
import { LocalStorage } from "utils/localStorage";
import { List } from "models/entities/list";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import * as environment from "../../config/environment.json";

@inject(Router, ListsService, UsersService, LocalStorage, EventAggregator, ConnectionTracker)
@connectTo()
export class Lists {
  private imageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private progressBar = new ProgressBar();
  private lists: Array<List>;
  private listsLoaded = false;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  private isReordering = false;
  private menuButtonIsLoading = false;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly usersService: UsersService,
    private readonly localStorage: LocalStorage,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setListsFromState();
      this.listsLoaded = true;
      this.progressBar.finish();
    });
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
  }

  setListsFromState() {
    this.lists = this.state.lists
      .filter((x) => !x.isArchived)
      .sort((a: List, b: List) => {
        return a.order - b.order;
      });
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  async reorder(changedArray: Array<List>, data) {
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
