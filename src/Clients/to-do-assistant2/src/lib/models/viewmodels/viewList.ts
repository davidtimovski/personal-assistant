import { Task } from "models/entities";
import { ListTask } from "./listTask";
import { SharingState } from "./sharingState";

export class ViewList {
  public tasks = new Array<ListTask>();
  public privateTasks = new Array<ListTask>();
  public completedTasks = new Array<ListTask>();
  public completedPrivateTasks = new Array<ListTask>();

  constructor(
    public id: number,
    public name: string,
    public isOneTimeToggleDefault: boolean,
    public sharingState: SharingState,
    public isArchived: boolean,
    public computedListType: string
  ) {}

  public setTasksFromState(tasks: Task[]) {
    this.tasks = tasks
      .filter((x) => !x.isCompleted && !x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }

  public setPrivateTasksFromState(tasks: Task[]) {
    this.privateTasks = tasks
      .filter((x) => !x.isCompleted && x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }

  public setCompletedTasksFromState(tasks: Task[]) {
    this.completedTasks = tasks
      .filter((x) => x.isCompleted && !x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }

  public setCompletedPrivateTasksFromState(tasks: Task[]) {
    this.completedPrivateTasks = tasks
      .filter((x) => x.isCompleted && x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }
}
