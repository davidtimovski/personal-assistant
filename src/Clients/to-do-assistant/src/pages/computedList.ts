import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ListsService } from "services/listsService";
import { TasksService } from "services/tasksService";
import { ViewComputedList } from "models/viewmodels/viewComputedList";
import { State } from "utils/state/state";
import { ListTask } from "models/viewmodels/listTask";
import { AppEvents } from "models/appEvents";
import { SoundPlayer } from "utils/soundPlayer";

@inject(Router, TasksService, I18N, EventAggregator)
@connectTo()
export class ComputedList {
  private type: string;
  private model = new ViewComputedList(0, "", null);
  private shadowTasks: ListTask[];
  private shadowPrivateTasks: ListTask[];
  private searchTasksText = "";
  private readonly soundPlayer = new SoundPlayer();
  private readonly computedListNameLookup;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly tasksService: TasksService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
      this.setModelFromState();
    });

    this.computedListNameLookup = {
      "high-priority": this.i18n.tr("highPriority"),
    };
  }

  activate(params: any) {
    this.type = params.type;
  }

  attached() {
    if (this.state.soundsEnabled) {
      this.soundPlayer.initialize();
    }

    if (!this.state.loading) {
      this.setModelFromState();
    }

    this.eventAggregator.subscribe(AppEvents.TaskCompletedChangedRemotely, (data: any) => {
      if (data.isCompleted) {
        const allTasks = this.model.tasks.concat(this.model.privateTasks);
        const task = allTasks.find((x) => x.id === data.id);
        this.complete(task, true);
      } else {
        const list = this.state.lists.find((x) => x.computedListType === this.type);
        const task = list.tasks.find((x) => x.id === data.id);

        if (task.isPrivate) {
          this.model.setPrivateTasks(list.tasks);
        } else {
          this.model.setTasks(list.tasks);
        }
      }
    });

    this.eventAggregator.subscribe(AppEvents.TaskDeletedRemotely, (data: any) => {
      const allTasks = this.model.tasks.concat(this.model.privateTasks);
      const task = allTasks.find((x) => x.id === data.id);

      this.complete(task, true);
    });
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.computedListType === this.type);
    if (!list || list.tasks.length === 0) {
      this.router.navigateToRoute("lists");
    } else {
      this.model.id = list.id;
      this.model.name = this.computedListNameLookup[list.computedListType];
      this.model.computedListType = list.computedListType;
      this.model.iconClass = ListsService.getComputedListIconClass(this.model.computedListType);

      this.model.setTasks(list.tasks);
      this.model.setPrivateTasks(list.tasks);

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

    if (this.searchTasksText.length > 0) {
      this.resetSearchFilter();
    }

    const list = this.state.lists.find((x) => x.computedListType === this.type);

    if (task.isOneTime) {
      if (!remote) {
        await this.tasksService.delete(task.id, task.listId);
      }

      this.executeAfterDelay(() => {
        if (this.model.tasks.length + this.model.privateTasks.length === 1) {
          this.router.navigateToRoute("lists");
        } else {
          if (task.isPrivate) {
            this.model.setPrivateTasks(list.tasks);
          } else {
            this.model.setTasks(list.tasks);
          }
        }
      }, startTime);
    } else {
      if (!remote) {
        await this.tasksService.complete(task.id, task.listId);
      }

      this.executeAfterDelay(() => {
        task.isChecked = false;
        task.isFading = false;

        if (this.model.tasks.length + this.model.privateTasks.length === 1) {
          this.router.navigateToRoute("lists");
        } else {
          if (task.isPrivate) {
            this.model.setPrivateTasks(list.tasks);
          } else {
            this.model.setTasks(list.tasks);
          }
        }
      }, startTime);
    }
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
}
