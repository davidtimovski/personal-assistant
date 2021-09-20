import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules } from "aurelia-validation";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ListsService } from "services/listsService";
import { TasksService } from "services/tasksService";
import { Task } from "models/entities/task";
import { ViewComputedList } from "models/viewmodels/viewComputedList";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import { ListTask } from "models/viewmodels/listTask";

@inject(Router, TasksService, ValidationController, EventAggregator)
@connectTo()
export class ComputedList {
  private type: string;
  private model = new ViewComputedList(0, "", null, [], []);
  private shadowTasks: ListTask[];
  private shadowPrivateTasks: ListTask[];
  private searchTasksText = "";
  private bleep = new Audio("/audio/bleep.mp3");
  private blop = new Audio("/audio/blop.mp3");
  state: State;

  constructor(
    private readonly router: Router,
    private readonly tasksService: TasksService,
    private readonly validationController: ValidationController,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setModelFromState();
    });

    this.validationController.validateTrigger = validateTrigger.manual;

    ValidationRules.ensure("newTaskName").required().on(this);
  }

  activate(params: any) {
    this.type = params.type;
  }

  attached() {
    if (this.state.soundsEnabled) {
      this.bleep.load();
      this.blop.load();
    }

    if (!this.state.loading) {
      this.setModelFromState();
    }
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.computedListType === this.type);
    if (!list || list.tasks.length === 0) {
      this.router.navigateToRoute("lists");
    } else {
      this.model.id = list.id;
      this.model.name = list.name;
      this.model.computedListType = list.computedListType;
      this.model.iconClass = ListsService.getComputedListIconClass(this.model.computedListType);

      this.model.tasks = list.tasks
        .filter((x) => !x.isCompleted && !x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        })
        .map((x) => {
          return ListTask.fromTask(x);
        });
      this.model.privateTasks = list.tasks
        .filter((x) => !x.isCompleted && x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        })
        .map((x) => {
          return ListTask.fromTask(x);
        });

      this.shadowTasks = this.model.tasks.slice();
      this.shadowPrivateTasks = this.model.privateTasks.slice();
    }
  }

  searchTasksInputChanged() {
    if (this.searchTasksText.trim().length > 0) {
      this.filterTasks();
    } else {
      this.model.tasks = this.shadowTasks.slice();
      this.model.privateTasks = this.shadowPrivateTasks.slice();
    }
  }

  clearFilter() {
    this.searchTasksText = "";
    this.filterTasks();
  }

  filterTasks() {
    this.model.tasks = this.shadowTasks.filter((task: ListTask) => {
      return task.name.toLowerCase().includes(this.searchTasksText.trim().toLowerCase());
    });
    this.model.privateTasks = this.shadowPrivateTasks.filter((task: ListTask) => {
      return task.name.toLowerCase().includes(this.searchTasksText.trim().toLowerCase());
    });
  }

  resetSearchFilter() {
    this.searchTasksText = "";
    this.model.tasks = this.shadowTasks.slice();
    this.model.privateTasks = this.shadowPrivateTasks.slice();
  }

  async complete(task: ListTask) {
    if (task.rightSideIsLoading) {
      return;
    }

    const startTime = new Date();

    if (this.state.soundsEnabled) {
      this.bleep.play();
    }

    task.rightSideIsLoading = true;
    task.isFading = true;

    if (this.searchTasksText.length > 0) {
      this.resetSearchFilter();
    }

    if (task.isOneTime) {
      await this.tasksService.delete(task.id);
      await Actions.deleteTask(task.id);

      this.executeAfterDelay(() => {
        if (this.model.tasks.length + this.model.privateTasks.length === 1) {
          this.router.navigateToRoute("lists");
        } else {
          this.setModelFromState();
        }
      }, startTime);

      if (this.state.soundsEnabled) {
        this.bleep.play();
      }
    } else {
      await this.tasksService.complete(task.id);
      await Actions.completeTask(task.id);

      this.executeAfterDelay(() => {
        task.rightSideIsLoading = false;
        task.isFading = false;

        if (this.model.tasks.length + this.model.privateTasks.length === 1) {
          this.router.navigateToRoute("lists");
        } else {
          this.setModelFromState();
        }
      }, startTime);
    }
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
}
