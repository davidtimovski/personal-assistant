import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { ListsService } from "services/listsService";
import { TasksService } from "services/tasksService";
import { ViewList } from "models/viewmodels/viewList";
import { SharingState } from "models/viewmodels/sharingState";
import { State } from "utils/state/state";
import { ListTask } from "models/viewmodels/listTask";
import { AppEvents } from "models/appEvents";
import { SoundPlayer } from "utils/soundPlayer";

@inject(Router, ListsService, TasksService, ValidationController, I18N, EventAggregator)
@connectTo()
export class List {
  private listId: number;
  private readonly model = new ViewList(0, "", false, SharingState.NotShared, null, null);
  private topDrawerIsOpen = false;
  private shareButtonText: string;
  private completedTasksAreVisible = false;
  private uncompleteDuplicateButtonVisible = false;
  private duplicateTaskMessageText: string;
  private duplicateTask: ListTask;
  private similarTasksMessageText: string;
  private similarTaskNames: string[] = [];
  private shadowTasks: ListTask[];
  private shadowPrivateTasks: ListTask[];
  private shadowCompletedTasks: ListTask[];
  private shadowCompletedPrivateTasks: ListTask[];
  private newTaskName = "";
  private isPrivate = false;
  private isOneTime: boolean;
  private newTaskIsLoading = false;
  private newTaskIsInvalid: boolean;
  private newTaskNameInput: HTMLInputElement;
  private lastEditedId: number;
  private isReordering = false;
  private isSearching = false;
  private readonly soundPlayer = new SoundPlayer();
  private editListButtonIsLoading = false;
  private addNewPlaceholderText: string;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly tasksService: TasksService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
      this.setModelFromState();
    });

    this.validationController.validateTrigger = validateTrigger.manual;

    ValidationRules.ensure("newTaskName").required().on(this);

    this.addNewPlaceholderText = this.i18n.tr("list.addNew");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.newTaskIsInvalid = false;
    });
  }

  activate(params: any) {
    this.listId = parseInt(params.id, 10);

    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    if (this.state.soundsEnabled) {
      this.soundPlayer.initialize();
    }

    if (!this.state.loading) {
      this.setModelFromState();
    }

    this.eventAggregator.subscribe(AppEvents.TaskCompletedChangedRemotely, (data: any) => {
      const allTasks = this.model.tasks
        .concat(this.model.privateTasks)
        .concat(this.model.completedTasks)
        .concat(this.model.completedPrivateTasks);
      const task = allTasks.find((x) => x.id === data.id);

      if (data.isCompleted) {
        this.complete(task, true);
      } else {
        this.uncomplete(task, true);
      }
    });

    this.eventAggregator.subscribe(AppEvents.TaskDeletedRemotely, (data: any) => {
      const allTasks = this.model.tasks
        .concat(this.model.privateTasks)
        .concat(this.model.completedTasks)
        .concat(this.model.completedPrivateTasks);
      const task = allTasks.find((x) => x.id === data.id);

      this.complete(task, true);
    });

    this.eventAggregator.subscribe(AppEvents.TaskReorderedRemotely, (data: any) => {
      const list = this.state.lists.find((x) => x.id === data.listId);
      const task = list.tasks.find((x) => x.id === data.id);

      if (task.isCompleted) {
        this.model.setCompletedTasksFromState(list.tasks);
      } else {
        this.model.setTasksFromState(list.tasks);
      }
    });
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.id === this.listId);
    if (!list) {
      this.router.navigateToRoute("notFound");
    } else {
      this.model.id = list.id;
      this.model.name = list.name;
      this.model.isOneTimeToggleDefault = list.isOneTimeToggleDefault;
      this.model.sharingState = list.sharingState;
      this.model.isArchived = list.isArchived;
      this.model.computedListType = list.computedListType;

      this.model.setTasksFromState(list.tasks);
      this.model.setPrivateTasksFromState(list.tasks);
      this.model.setCompletedTasksFromState(list.tasks);
      this.model.setCompletedPrivateTasksFromState(list.tasks);

      this.isOneTime = this.model.isOneTimeToggleDefault;
      this.shareButtonText =
        this.model.sharingState === SharingState.NotShared
          ? this.i18n.tr("list.shareList")
          : this.i18n.tr("list.members");
    }
  }

  toggleTopDrawer() {
    this.topDrawerIsOpen = !this.topDrawerIsOpen;
  }

  closeDrawer() {
    if (this.topDrawerIsOpen) {
      this.topDrawerIsOpen = false;
    }
  }

  async reorder(changedArray: ListTask[], data) {
    const id: number = changedArray[data.toIndex].id;

    const oldOrder = ++data.fromIndex;
    const newOrder = ++data.toIndex;

    await this.tasksService.reorder(id, this.listId, oldOrder, newOrder);

    this.isReordering = false;
  }

  isSearchingToggleChanged() {
    if (this.isSearching) {
      this.newTaskIsInvalid = false;
      this.completedTasksAreVisible = true;

      this.shadowTasks = this.model.tasks.slice();
      this.shadowPrivateTasks = this.model.privateTasks.slice();
      this.shadowCompletedTasks = this.model.completedTasks.slice();
      this.shadowCompletedPrivateTasks = this.model.completedPrivateTasks.slice();

      this.addNewPlaceholderText = this.i18n.tr("list.searchTasks");
      this.filterTasks();
      this.newTaskNameInput.focus();
    } else {
      this.resetSearchFilter(false);
    }
  }

  isOneTimeToggleChanged() {
    this.newTaskNameInput.focus();
  }

  isPrivateToggleChanged() {
    this.addNewPlaceholderText = this.isPrivate ? this.i18n.tr("list.addNewPrivate") : this.i18n.tr("list.addNew");
    this.newTaskNameInput.focus();
  }

  newTaskNameInputChanged(event: KeyboardEvent) {
    if (this.isSearching) {
      if (this.newTaskName.trim().length > 0) {
        this.filterTasks();
      } else {
        this.model.tasks = this.shadowTasks.slice();
        this.model.privateTasks = this.shadowPrivateTasks.slice();
        this.model.completedTasks = this.shadowCompletedTasks.slice();
        this.model.completedPrivateTasks = this.shadowCompletedPrivateTasks.slice();
      }
    } else if (event.key !== "Enter") {
      this.newTaskIsInvalid = false;
      this.duplicateTask = null;
      this.similarTaskNames = [];
    }
  }

  filterTasks() {
    const searchText = this.newTaskName.trim().toLowerCase();

    const mapFunction = (task: ListTask) => {
      const index = task.name.toLowerCase().indexOf(searchText);
      if (index >= 0) {
        return { task: task, index: index };
      }

      return null;
    };

    const substringPositionThenOrder = (a: { task: ListTask; index: number }, b: { task: ListTask; index: number }) => {
      if (a.index < b.index) return -1;
      if (a.index > b.index) return 1;
      return a.task.order - b.task.order;
    };

    this.model.tasks = this.shadowTasks
      .map(mapFunction)
      .filter((x) => x)
      .sort(substringPositionThenOrder)
      .map((x) => x.task);

    this.model.privateTasks = this.shadowPrivateTasks
      .map(mapFunction)
      .filter((x) => x)
      .sort(substringPositionThenOrder)
      .map((x) => x.task);

    this.model.completedTasks = this.shadowCompletedTasks
      .map(mapFunction)
      .filter((x) => x)
      .sort(substringPositionThenOrder)
      .map((x) => x.task);

    this.model.completedPrivateTasks = this.shadowCompletedPrivateTasks
      .map(mapFunction)
      .filter((x) => x)
      .sort(substringPositionThenOrder)
      .map((x) => x.task);
  }

  resetSearchFilter(resetTaskName: boolean = true) {
    if (resetTaskName) {
      this.newTaskName = "";
    }

    this.isSearching = false;
    this.model.tasks = this.shadowTasks.slice();
    this.model.privateTasks = this.shadowPrivateTasks.slice();
    this.model.completedTasks = this.shadowCompletedTasks.slice();
    this.model.completedPrivateTasks = this.shadowCompletedPrivateTasks.slice();
    this.addNewPlaceholderText = this.i18n.tr("list.addNew");
  }

  toggleCompletedTasksAreVisible() {
    this.completedTasksAreVisible = !this.completedTasksAreVisible;
  }

  async create() {
    if (this.isSearching) {
      return;
    }

    this.newTaskIsLoading = true;
    this.lastEditedId = 0;

    const result: ControllerValidateResult = await this.validationController.validate();

    this.newTaskIsInvalid = !result.valid;

    if (result.valid) {
      if (this.newTaskIsDuplicate()) {
        this.newTaskIsLoading = false;
        this.newTaskIsInvalid = true;

        if (this.duplicateTask.isCompleted) {
          this.duplicateTaskMessageText = this.i18n.tr("list.alreadyExistsUncomplete");
          this.uncompleteDuplicateButtonVisible = true;
        } else {
          this.duplicateTaskMessageText = this.i18n.tr("list.alreadyExists");
          this.uncompleteDuplicateButtonVisible = false;
        }
      } else {
        if (this.similarTaskNames.length) {
          this.similarTaskNames = [];
        } else {
          this.similarTaskNames = this.findSimilarTasks(this.newTaskName);
        }

        if (this.similarTaskNames.length) {
          this.newTaskIsLoading = false;

          this.similarTasksMessageText = this.i18n.tr("list.similarTasksExist", {
            taskNames: this.similarTaskNames.join(", "),
          });
        } else {
          this.newTaskIsInvalid = false;
          this.duplicateTask = null;
          this.similarTaskNames = [];

          try {
            await this.tasksService.create(
              this.model.id,
              this.newTaskName,
              this.isOneTime,
              this.isPrivate,
              this.listsService
            );
            this.newTaskIsLoading = false;
            this.newTaskName = "";

            const list = this.state.lists.find((x) => x.id === this.listId);

            if (this.isPrivate) {
              this.model.setPrivateTasksFromState(list.tasks);
            } else {
              this.model.setTasksFromState(list.tasks);
            }

            if (this.state.soundsEnabled) {
              this.soundPlayer.playBlop();
            }
          } catch {
            this.newTaskIsLoading = false;
            this.newTaskIsInvalid = true;
          }
        }
      }
    } else {
      this.newTaskIsLoading = false;
    }
  }

  async complete(task: ListTask, remote = false) {
    if (task.isChecked) {
      return;
    }

    const startTime = new Date();

    if (!remote && this.state.soundsEnabled) {
      this.soundPlayer.playBleep();
    }

    task.isChecked = true;
    task.isFading = true;
    this.lastEditedId = 0;

    if (this.isSearching) {
      this.resetSearchFilter();
    }

    const list = this.state.lists.find((x) => x.id === this.listId);

    if (task.isOneTime) {
      if (!remote) {
        await this.tasksService.delete(task.id, this.listId);
      }

      this.executeAfterDelay(() => {
        if (task.isPrivate) {
          this.model.setPrivateTasksFromState(list.tasks);
        } else {
          this.model.setTasksFromState(list.tasks);
        }
      }, startTime);
    } else {
      if (!remote) {
        await this.tasksService.complete(task.id, this.listId);
      }

      this.executeAfterDelay(() => {
        task.isChecked = false;
        task.isFading = false;

        if (task.isPrivate) {
          this.model.setPrivateTasksFromState(list.tasks);
          this.model.setCompletedPrivateTasksFromState(list.tasks);
        } else {
          this.model.setTasksFromState(list.tasks);
          this.model.setCompletedTasksFromState(list.tasks);
        }
      }, startTime);
    }
  }

  async uncomplete(task: ListTask, remote = false) {
    if (task.isChecked) {
      return;
    }

    const startTime = new Date();

    if (!remote && this.state.soundsEnabled) {
      this.soundPlayer.playBlop();
    }

    task.isChecked = true;
    task.isFading = true;
    this.lastEditedId = 0;

    if (this.isSearching) {
      this.resetSearchFilter();
    }

    if (!remote) {
      await this.tasksService.uncomplete(task.id, this.listId);
    }

    this.executeAfterDelay(() => {
      task.isChecked = false;
      task.isFading = false;

      const list = this.state.lists.find((x) => x.id === this.listId);

      if (task.isPrivate) {
        this.model.setCompletedPrivateTasksFromState(list.tasks);
        this.model.setPrivateTasksFromState(list.tasks);
      } else {
        this.model.setCompletedTasksFromState(list.tasks);
        this.model.setTasksFromState(list.tasks);
      }
    }, startTime);
  }

  /** Used to delay UI update if server responds too quickly */
  executeAfterDelay(callback: () => void, startTime: Date) {
    const delayMs = 600;
    const timeTaken = new Date().getTime() - startTime.getTime();
    const sleepTime = delayMs - timeTaken;

    window.setTimeout(() => {
      callback();
    }, Math.max(sleepTime, 0));
  }

  editList() {
    this.editListButtonIsLoading = true;
    this.router.navigateToRoute("editList", { id: this.model.id });
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }

  back() {
    if (this.model.isArchived) {
      this.router.navigateToRoute("archivedLists");
    } else {
      this.router.navigateToRoute("lists");
    }
  }

  async restore() {
    await this.listsService.setIsArchived(this.model.id, false);

    this.router.navigateToRoute("listsEdited", { editedId: this.model.id });
  }

  async uncompleteDuplicate() {
    this.newTaskName = "";
    this.newTaskIsInvalid = false;

    await this.uncomplete(this.duplicateTask);
    this.lastEditedId = this.duplicateTask.id;
    this.duplicateTask = null;
  }

  newTaskIsDuplicate(): boolean {
    const tasks = this.model.tasks.concat(this.model.completedTasks);

    for (let task of tasks) {
      if (task.name.toLowerCase() === this.newTaskName.trim().toLowerCase()) {
        this.duplicateTask = task;
        return true;
      }
    }

    this.duplicateTask = null;
    return false;
  }

  findSimilarTasks(newTaskName: string): string[] {
    const newTaskNameWords = newTaskName.split(" ").filter((word) => word.length > 3);
    const allTaskNames = this.model.tasks
      .concat(this.model.privateTasks)
      .concat(this.model.completedTasks)
      .concat(this.model.completedPrivateTasks)
      .map((task) => {
        return task.name;
      });

    const similarTasks = [];
    for (let name of allTaskNames) {
      for (let newTaskWord of newTaskNameWords) {
        if (name.toLowerCase().includes(newTaskWord.toLowerCase())) {
          similarTasks.push(name);
        }
      }
    }
    return similarTasks;
  }

  @computedFrom("duplicateTask")
  get duplicateAlertIsVisible(): boolean {
    return this.duplicateTask != null;
  }

  hideDuplicateTaskAlert() {
    this.newTaskIsInvalid = false;
    this.duplicateTask = null;
  }

  @computedFrom("similarTaskNames")
  get similarTasksAlertIsVisible(): boolean {
    return !!this.similarTaskNames.length;
  }
}
