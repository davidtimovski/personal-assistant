import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
} from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { ListsService } from "services/listsService";
import { TasksService } from "services/tasksService";
import { Task } from "models/entities/task";
import { ViewList } from "models/viewmodels/viewList";
import { LocalStorage } from "utils/localStorage";
import { SharingState } from "models/viewmodels/sharingState";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";

@inject(
  Router,
  ListsService,
  TasksService,
  ValidationController,
  I18N,
  EventAggregator,
  LocalStorage
)
@connectTo()
export class List {
  private listId: number;
  private model = new ViewList(
    0,
    "",
    false,
    SharingState.NotShared,
    null,
    new Array<Task>(),
    new Array<Task>(),
    new Array<Task>(),
    new Array<Task>()
  );
  private topDrawerIsOpen = false;
  private shareButtonText: string;
  private completedTasksAreVisible = false;
  private uncompleteDuplicateButtonVisible = false;
  private duplicateTaskMessageText: string;
  private duplicateTask: Task;
  private similarTasksMessageText: string;
  private similarTaskNames: string[] = [];
  private shadowTasks: Task[];
  private shadowPrivateTasks: Task[];
  private shadowCompletedTasks: Task[];
  private shadowCompletedPrivateTasks: Task[];
  private newTaskName = "";
  private isPrivate = false;
  private isOneTime: boolean;
  private newTaskIsLoading = false;
  private newTaskIsInvalid: boolean;
  private newTaskNameInput: HTMLInputElement;
  private lastEditedId: number;
  private isReordering = false;
  private isSearching = false;
  private bleep = new Audio("/audio/bleep.mp3");
  private blop = new Audio("/audio/blop.mp3");
  private editListButtonIsLoading = false;
  private addNewPlaceholderText: string;
  private emptyListMessage: string;
  private soundsEnabled = true;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly tasksService: TasksService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorage
  ) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setModelFromState();
    });

    this.validationController.validateTrigger = validateTrigger.manual;

    ValidationRules.ensure("newTaskName").required().on(this);

    this.addNewPlaceholderText = this.i18n.tr("list.add");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.newTaskIsInvalid = false;
    });
  }

  activate(params: any) {
    this.listId = parseInt(params.id, 10);

    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.soundsEnabled = this.localStorage.getSoundsEnabled();
  }

  attached() {
    if (this.soundsEnabled) {
      this.bleep.load();
      this.blop.load();
    }

    if (!this.state.loading) {
      this.setModelFromState();
    }
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.id === this.listId);
    if (list === null) {
      this.router.navigateToRoute("notFound");
    } else {
      this.model.id = list.id;
      this.model.name = list.name;
      this.model.isOneTimeToggleDefault = list.isOneTimeToggleDefault;
      this.model.sharingState = list.sharingState;
      this.model.isArchived = list.isArchived;
      this.model.tasks = list.tasks
        .filter((x) => !x.isCompleted && !x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      this.model.privateTasks = list.tasks
        .filter((x) => !x.isCompleted && x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      this.model.completedTasks = list.tasks
        .filter((x) => x.isCompleted && !x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      this.model.completedPrivateTasks = list.tasks
        .filter((x) => x.isCompleted && x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });

      this.isOneTime = this.model.isOneTimeToggleDefault;
      this.emptyListMessage = this.i18n.tr("list.emptyListMessage");
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

  async reorder(changedArray: Array<Task>, newOrder) {
    const id: number = changedArray[newOrder.toIndex].id;

    await this.tasksService.reorder(
      id,
      ++newOrder.fromIndex,
      ++newOrder.toIndex
    );

    this.isReordering = false;

    await Actions.getLists(this.listsService);
  }

  isSearchingToggleChanged() {
    if (this.isSearching) {
      this.newTaskIsInvalid = false;
      this.completedTasksAreVisible = true;

      this.shadowTasks = this.model.tasks.slice();
      this.shadowPrivateTasks = this.model.privateTasks.slice();
      this.shadowCompletedTasks = this.model.completedTasks.slice();
      this.shadowCompletedPrivateTasks = this.model.completedPrivateTasks.slice();

      this.addNewPlaceholderText = this.i18n.tr("list.search");
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
    this.addNewPlaceholderText = this.isPrivate
      ? this.i18n.tr("list.addPrivate")
      : this.i18n.tr("list.add");
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
    } else if (
      event.charCode !== 13 &&
      event.which !== 13 &&
      event.key !== "Enter"
    ) {
      this.newTaskIsInvalid = false;
      this.duplicateTask = null;
      this.similarTaskNames = [];
    }
  }

  filterTasks() {
    this.model.tasks = this.shadowTasks.filter((task: Task) => {
      return task.name
        .toLowerCase()
        .includes(this.newTaskName.trim().toLowerCase());
    });
    this.model.privateTasks = this.shadowPrivateTasks.filter((task: Task) => {
      return task.name
        .toLowerCase()
        .includes(this.newTaskName.trim().toLowerCase());
    });
    this.model.completedTasks = this.shadowCompletedTasks.filter(
      (task: Task) => {
        return task.name
          .toLowerCase()
          .includes(this.newTaskName.trim().toLowerCase());
      }
    );
    this.model.completedPrivateTasks = this.shadowCompletedPrivateTasks.filter(
      (task: Task) => {
        return task.name
          .toLowerCase()
          .includes(this.newTaskName.trim().toLowerCase());
      }
    );
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
    this.addNewPlaceholderText = this.i18n.tr("list.add");
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
          this.duplicateTaskMessageText = this.i18n.tr(
            "list.alreadyExistsUncomplete"
          );
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

          this.similarTasksMessageText = this.i18n.tr(
            "list.similarTasksExist",
            {
              taskNames: this.similarTaskNames.join(", "),
            }
          );
        } else {
          this.newTaskIsInvalid = false;
          this.duplicateTask = null;
          this.similarTaskNames = [];

          try {
            const id = await this.tasksService.create(
              this.model.id,
              this.newTaskName,
              this.isOneTime,
              this.isPrivate
            );
            this.newTaskIsLoading = false;
            this.newTaskName = "";

            await Actions.getLists(this.listsService);

            const list = this.state.lists.find((x) => x.id === this.listId);
            const task = list.tasks.find((x) => x.id === id);

            if (this.isPrivate) {
              this.model.privateTasks.unshift(task);
            } else {
              this.model.tasks.unshift(task);
            }

            if (this.soundsEnabled) {
              this.blop.play();
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

  async complete(task: Task) {
    if (task.rightSideIsLoading) {
      return;
    }

    const startTime = new Date();

    if (this.soundsEnabled) {
      this.bleep.play();
    }

    task.rightSideIsLoading = true;
    this.lastEditedId = 0;

    if (this.isSearching) {
      this.resetSearchFilter();
    }

    if (task.isOneTime) {
      await this.tasksService.delete(task.id);
      await Actions.deleteTask(this.listId, task.id);

      this.executeAfterDelay(() => {
        this.setModelFromState();
      }, startTime);

      if (this.soundsEnabled) {
        this.bleep.play();
      }
    } else {
      await this.tasksService.complete(task.id);
      await Actions.completeTask(this.listId, task.id);

      this.executeAfterDelay(() => {
        task.rightSideIsLoading = false;
        this.setModelFromState();
      }, startTime);
    }
  }

  async uncomplete(task: Task) {
    if (task.rightSideIsLoading) {
      return;
    }

    const startTime = new Date();

    if (this.soundsEnabled) {
      this.blop.play();
    }

    task.rightSideIsLoading = true;
    this.lastEditedId = 0;

    if (this.isSearching) {
      this.resetSearchFilter();
    }

    await this.tasksService.uncomplete(task.id);
    await Actions.uncompleteTask(this.listId, task.id);

    this.executeAfterDelay(() => {
      task.rightSideIsLoading = false;
      this.setModelFromState();
    }, startTime);
  }

  // Used to delay UI update if server responds too quickly
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

    await Actions.getLists(this.listsService);

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
    const newTaskNameWords = newTaskName
      .split(" ")
      .filter((word) => word.length > 3);
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
