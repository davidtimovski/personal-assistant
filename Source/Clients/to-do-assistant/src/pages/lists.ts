import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ListsService } from "services/listsService";
import { UsersService } from "services/usersService";
import { IndexedDBHelper } from "utils/indexedDBHelper";
import { LocalStorage } from "utils/localStorage";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { SimpleList } from "models/viewmodels/simpleList";
import { ProgressBar } from "../../../shared/src/models/progressBar";
import * as environment from "../../config/environment.json";

@inject(
  Router,
  ListsService,
  UsersService,
  IndexedDBHelper,
  LocalStorage,
  ConnectionTracker
)
export class Lists {
  private imageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private progressBar = new ProgressBar();
  private lists = new Array<SimpleList>();
  private listsLoaded = false;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  private isReordering = false;
  private menuButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly usersService: UsersService,
    private readonly indexedDBHelper: IndexedDBHelper,
    private readonly localStorage: LocalStorage,
    private readonly connTracker: ConnectionTracker
  ) {}

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.imageUri = this.localStorage.getProfileImageUri();
  }

  attached() {
    this.indexedDBHelper.getLists().then((cachedLists: Array<SimpleList>) => {
      if (this.lists.length === 0) {
        this.lists = cachedLists;
        this.listsLoaded = true;
      }
    });

    if (this.localStorage.isStale("data")) {
      this.progressBar.start();

      this.listsService
        .getAll()
        .then((lists: Array<SimpleList>) => {
          if (!this.listsAreSame(this.lists, lists)) {
            this.lists = lists;
            this.listsLoaded = true;
            this.indexedDBHelper.createListsWithTasks(lists);
          }
          this.localStorage.setDataLastLoad(new Date());
        })
        .finally(() => {
          this.progressBar.finish();
        });
    }

    if (this.localStorage.isStale("profileImageUri")) {
      this.usersService.getProfileImageUri().then((imageUri) => {
        if (this.imageUri !== imageUri) {
          this.imageUri = imageUri;
        }
      });
    }
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  async reorder(changedArray: Array<SimpleList>, newOrder) {
    const id: number = changedArray[newOrder.toIndex].id;

    await this.listsService.reorder(
      id,
      ++newOrder.fromIndex,
      ++newOrder.toIndex
    );
    this.isReordering = false;

    this.listsService.getAll().then((lists: Array<SimpleList>) => {
      this.indexedDBHelper.createListsWithTasks(lists);
      this.localStorage.setDataLastLoad(new Date());
    });
  }

  listsAreSame(a: Array<SimpleList>, b: Array<SimpleList>): boolean {
    if (a === undefined || a.length !== b.length) {
      return false;
    }

    for (let i = 0; i < a.length; i++) {
      const areEqual =
        a[i].id === b[i].id &&
        a[i].name === b[i].name &&
        a[i].icon === b[i].icon &&
        a[i].sharingState === b[i].sharingState &&
        a[i].order === b[i].order &&
        JSON.stringify(a[i].tasks) === JSON.stringify(b[i].tasks);

      if (!areEqual) {
        return false;
      }
    }

    return true;
  }

  sync() {
    this.progressBar.start();

    this.listsService
      .getAll()
      .then((lists: Array<SimpleList>) => {
        this.lists = lists;
        this.indexedDBHelper.createListsWithTasks(lists);
        this.localStorage.setDataLastLoad(new Date());
      })
      .finally(() => {
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
